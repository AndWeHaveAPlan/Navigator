using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Navigator.Core;
using Navigator.Core.Attributes;
using Navigator.Core.Client;
using Navigator.Crow.DataTypes;

namespace Navigator.Crow.Controllers
{
    [NavigatorController("crow")]
    public class SelfTestController
    {
        private readonly NavigatorClientFactory _clientFactory;

        public SelfTestController(NavigatorClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [NavigatorMethod]
        public async Task<MathResponse> PartySoft(int totalMessages, string serviceName = "crow")
        {
            var client = _clientFactory.CreateClient();

            client.BaseAddress = new NavigatorAddress(serviceName + "::Exchange");

            var start = DateTime.Now;

            for (var i = 0; i < totalMessages; i++)
            {
                await client.Post<GiveResponse>("Exchange", new GiveRequest
                {
                    ItemCount = 1,
                    ItemName = "Vertebra Shackle"
                });
            }

            var workTime = DateTime.Now - start;

            return new MathResponse { Result = workTime.TotalMilliseconds };
        }

        [NavigatorMethod]
        public MathResponse PartyHard(int totalMessages, string serviceName = "crow")
        {
            var client = _clientFactory.CreateClient();

            client.BaseAddress = new NavigatorAddress(serviceName + "::Exchange");

            var start = DateTime.Now;

            List<Task> tasks = new List<Task>();

            for (var i = 0; i < totalMessages; i++)
            {
                var t = client.Post<GiveResponse>("Exchange", new GiveRequest
                {
                    ItemCount = 1,
                    ItemName = "Vertebra Shackle"
                });
                tasks.Add(t);
            }

            Task.WaitAll(tasks.ToArray());

            var workTime = DateTime.Now - start;

            return new MathResponse { Result = workTime.TotalMilliseconds };
        }
    }
}
