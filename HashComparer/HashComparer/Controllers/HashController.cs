using HashComparer.Model;
using HashComparer.Services;
using HashComparer.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace HashComparer.Controllers
{
    [Route("api/hash")]
    [ApiController]
    public class HashController : ControllerBase
    {
        // todo: Change to IOptions
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private readonly IHasher _hasher;

        public HashController(IConfiguration configuration, IHasher hasher)
        {
            _configuration = configuration;
            _hasher = hasher;
        }

        [HttpPost]
        public IActionResult CompareHash([FromBody] HashMessageRequest request)
        {
            IActionResult response;

            var specialKeyFromConfig = _configuration[GlobalVariables.SpecialKeyPath];
            var keyIdFromConfig = _configuration[GlobalVariables.KeyIdPath];

            string resultInHMAC = string.Empty;
            string signatureInHMAC = string.Empty;

            try
            {
                var eventSignatures = string.Empty;

                if (Request.Headers.TryGetValue("Event-Signature", out StringValues headerValues))
                {
                    eventSignatures = headerValues.FirstOrDefault();
                }

                if (string.IsNullOrEmpty(eventSignatures))
                {
                    response = BadRequest(@"No signature event provided. Please, provide ""Event-Signature"" header.");
                }

                // in case if we have two or more signatures provided
                var eventSignaturesList = eventSignatures.Split(",");

                // parts of the event signature {keyId}/{hashFunction}/{signature} in an array
                var lastEventSignatureParts = eventSignaturesList[int.Parse(keyIdFromConfig) - 1].Split("/");

                var keyId = lastEventSignatureParts[0];
                var hashFunction = lastEventSignatureParts[1];

                signatureInHMAC = lastEventSignatureParts[2];
                resultInHMAC = _hasher.Hash(request.Message, specialKeyFromConfig.ToCharArray());
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            if (signatureInHMAC == resultInHMAC)
            {
                response = Ok("Hashes are equal.");
            }
            else
            {
                response = BadRequest("Hashes are NOT equal.");
            }

            //return response;
            return Ok(new
            {
                signatureInHMAC,
                resultInHMAC
            });
        }
    }
}
