using Navigator.Core.Client;

namespace Navigator.Crow.Services
{
    public class CrowClientService
    {
        private static NavigatorClient _client;

        public NavigatorClient Client => _client;

        public static void SetClient(NavigatorClient client)
        {
            _client = client;
        }
    }
}
