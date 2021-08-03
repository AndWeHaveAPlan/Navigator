using Navigator.Core.Attributes;
using Navigator.Core.Client;

namespace Navigator.Crow.Controllers
{
    [NavigatorController()]
    public class EventManagerController
    {
        private readonly NavigatorClient _client;

        public EventManagerController(NavigatorClient client)
        {
            _client = client;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NavigatorMethod]
        public string Fire()
        {
            _client.Publish("ACK", "pickle-pee").Wait();
            return "";
        }
    }
}
