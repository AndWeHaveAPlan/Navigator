using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Navigator.Core;
using Navigator.Core.Client;

namespace Navigator.HttpGateway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NavigatorGateController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<NavigatorGateController> _logger;
        private readonly NavigatorClient _navigatorClient;

        public NavigatorGateController(ILogger<NavigatorGateController> logger, NavigatorClient navigatorClient)
        {
            _logger = logger;
            _navigatorClient = navigatorClient;
        }

        [HttpPost("{service}/{interface}/{method}")]
        public async Task<object> Get(string service, string @interface, string method, [FromBody] object[] body)
        {
            var result = await _navigatorClient.Post<object>(new NavigatorAddress(service, @interface, method), body);
            return result;
        }
    }
}
