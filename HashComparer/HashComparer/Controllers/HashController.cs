using HashComparer.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HashComparer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HashController : ControllerBase
    {
        public IConfiguration Configuration { get; set; }

        public HashController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> CompareHash([FromBody] Request request)
        {

            return Ok(request);
        }
    }
}
