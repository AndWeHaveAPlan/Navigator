using System.Threading.Tasks;

namespace Navigator.Core.Client
{
    public class NavigatorClientFactory
    {
        /// <summary>
        /// 
        /// </summary>
        private readonly ImmateriumHost _host;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="host"></param>
        public NavigatorClientFactory(ImmateriumHost host)
        {
            _host = host;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public NavigatorClient CreateClient()
        {
            return _host.CreateClient();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        public NavigatorClient CreateClient(string jwtToken)
        {
            return _host.CreateClient(jwtToken);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<NavigatorClient> CreateClient(string username, string password)
        {
            return await _host.CreateClient(username, password);
        }
    }
}
