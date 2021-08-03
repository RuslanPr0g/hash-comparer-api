using HashComparer.Model;
using HashComparer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;
using System.Text.Json;

namespace HashComparer.Controllers
{
    [Route("api/hash")]
    [ApiController]
    public class HashController : ControllerBase
    {
        private readonly HashingConfig _hashingConfiguration;
        private readonly ILogger _logger;
        private readonly IHasher _hasher;

        public HashController(IOptions<HashingConfig> configuration, IHasher hasher)
        {
            _hashingConfiguration = configuration.Value;
            _hasher = hasher;
        }

        [HttpPost]
        public IActionResult CompareHash([FromBody] HashMessageRequest request)
        {
            IActionResult response;

            var resultInHMAC = string.Empty;
            var signatureInHMAC = string.Empty;

            try
            {
                var eventSignatures = string.Empty;
                var specialKeyFromConfig = _hashingConfiguration.SpecialKey;
                var keyIdFromConfig = _hashingConfiguration.KeyId;

                if (Request.Headers.TryGetValue("Event-Signature", out var headerValues))
                {
                    eventSignatures = headerValues.FirstOrDefault();
                }

                if (string.IsNullOrEmpty(eventSignatures))
                {
                    response = BadRequest(@"No signature event provided. Please, provide ""Event-Signature"" header.");
                }

                // in case if we have two or more signatures provided
                if (eventSignatures != null)
                {
                    var eventSignaturesList = eventSignatures.Split(",");

                    // parts of the event signature {keyId}/{hashFunction}/{signature} in an array
                    var lastEventSignatureParts = eventSignaturesList[int.Parse(keyIdFromConfig) - 1].Split("/");

                    var keyId = lastEventSignatureParts[0];
                    var hashFunction = lastEventSignatureParts[1];

                    signatureInHMAC = lastEventSignatureParts[2];
                }

                resultInHMAC = _hasher.Hash(JsonSerializer.Serialize(request), specialKeyFromConfig.ToCharArray());
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }

            if (signatureInHMAC == resultInHMAC)
            {
                response = Ok(new
                {
                    signatureInHMAC,
                    resultInHMAC,
                    equal = true
                });
            }
            else
            {
                response = BadRequest(new
                {
                    signatureInHMAC,
                    resultInHMAC,
                    equal = false
                });
            }

            return response;
        }
    }
}
