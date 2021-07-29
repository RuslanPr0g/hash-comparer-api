using HashComparer.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace HashComparer.Controllers
{
    [Route("api/hash")]
    [ApiController]
    public class HashController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public HashController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Hello world");
        }

        [HttpPost]
        public IActionResult CompareHash([FromBody] Request request)
        {
            var eventSignatures = string.Empty;
            var specialKeyFromConfig = _configuration["Hashing:SpecialKey"];
            var keyIdFromConfig = _configuration["Hashing:KeyId"];
            string resultInHMAC;
            string signatureInHMAC;
            try
            {
                if (Request.Headers.TryGetValue("Event-Signature", out StringValues headerValues))
                {
                    eventSignatures = headerValues.FirstOrDefault();
                }

                if (eventSignatures is null ||
                    eventSignatures == string.Empty)
                {
                    return BadRequest(@"No signature event provided. Please, provide ""Event-Signature"" header.");
                }

                // in case if we have two or more signatures provided
                var eventSignaturesList = eventSignatures.Split(",");

                // parts of the last event signature {keyId}/{hashFunction}/{signature} in an array
                var lastEventSignatureParts = eventSignaturesList[^1].Split("/");

                var keyId = lastEventSignatureParts[0];
                var hashFunction = lastEventSignatureParts[1];

                signatureInHMAC = lastEventSignatureParts[2];
                resultInHMAC = CalculateHMAC(request.Message, specialKeyFromConfig.ToCharArray());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return BadRequest("Something went in a wrong way.");
            }

            return signatureInHMAC == resultInHMAC ? Ok("Hashes are equal.") : BadRequest("Hashes are NOT equal.");
        }

        private static string CalculateHMAC(string data, char[] keyCharArray)
        {
            byte[] keyEncoded = Encoding.ASCII.GetBytes(keyCharArray);
            HMACSHA256 myhmacsha256 = new(keyEncoded);
            byte[] dataByteArray = Encoding.ASCII.GetBytes(data);
            MemoryStream dataByteArrayStream = new(dataByteArray);
            string resultInHMAC = myhmacsha256.ComputeHash(dataByteArrayStream)
                .Aggregate("", (s, e) =>
                {
                    return $"{s}{e:x2}";
                }, s => s);
            return resultInHMAC;
        }
    }
}
