using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Navigator.Attributes;
using Navigator.Client;
using Navigator.Playground.Models;

namespace Navigator.Playground.Controllers
{
    [NavigatorController("crow", Interface = "echo")]
    public class EchoController
    {
        private readonly ILogger<EchoController> _logger;
        private readonly NavigatorClient _navigatorClient;

        public EchoController(ILogger<EchoController> logger, NavigatorClient navigatorClient)
        {
            _logger = logger;
            _navigatorClient = navigatorClient;
        }

        [NavigatorMethod()]
        public string String(string something)
        {
            return something;
        }

        [NavigatorMethod()]
        public string Reverse(string something)
        {
            return new string(something.Reverse().ToArray());
        }

        [NavigatorMethod()]
        public string Concat(params string[] strings)
        {
            return string.Join("+", strings);
        }

        [NavigatorMethod()]
        public string Sum(int i1, int i2 = 40)
        {
            return (i1 + i2).ToString();
        }

        [NavigatorMethod()]
        public SimpleClass Object(int i1, string s2)
        {
            return new SimpleClass() { I1 = i1 + 1, S1 = s2.ToUpper() };
        }
    }
}
