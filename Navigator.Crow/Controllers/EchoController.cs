using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using Navigator.Core.Attributes;

namespace Navigator.Crow.Controllers
{
    [NavigatorController(Interface = "echo")]
    public class EchoController
    {
        private readonly ILogger<EchoController> _logger;

        public EchoController(ILogger<EchoController> logger)
        {
            _logger = logger;
        }

        [NavigatorMethod]
        public string Echo(string anyString)
        {
            _logger.LogInformation(anyString);
            return anyString;
        }

        [NavigatorMethod]
        public string Reverse(string anyString)
        {
            _logger.LogInformation(anyString);
            return new string(anyString.Reverse().ToArray());
        }

        [NavigatorMethod]
        public string Null(string anyString)
        {
            _logger.LogInformation(anyString);
            return null;
        }

        [NavigatorMethod]
        public DateTimeResponse CurrentTime()
        {
            return new DateTimeResponse
            {
                Result = DateTime.UtcNow
            };
        }
    }

    public class DateTimeResponse
    {
        public DateTime Result { get; set; }
    }
}
