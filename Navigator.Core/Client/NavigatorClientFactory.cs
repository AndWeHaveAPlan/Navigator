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
    }
}
