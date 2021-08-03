using Navigator.Core.Attributes;
using Navigator.Core.Client;

namespace Navigator.Crow.Controllers
{
    [NavigatorController("crow")]
    public class EventManagerController 
    {
        private readonly NavigatorClientFactory _clientFactory;

        public EventManagerController(NavigatorClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
            var client = _clientFactory.CreateClient();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [NavigatorMethod]
        public string Fire()
        {
            _clientFactory.CreateClient().Publish("ACK", "pickle-pee").Wait();
            return "";
        }
    }
}
