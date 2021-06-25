using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navigator.Client;
using Navigator.Core;
//using Navigator.Exceptions.Middleware;
using Navigator.Pipeline;
using Navigator.Pipeline.Middleware;
using System;
using System.Reflection;

namespace Navigator.Builder
{
    public class ImmateriumHostBuilder
    {
        private IStartup _startup;

        private Tuple<string, string> _userPass;

        private readonly ImmateriumHost _host;

        /// <summary>
        /// 
        /// </summary>
        internal ImmateriumHostBuilder()
        {
            _host = new ImmateriumHost();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startup"></param>
        /// <returns></returns>
        public ImmateriumHostBuilder UseStartup(IStartup startup)
        {
            _startup = startup;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ImmateriumHostBuilder UseServiceName(string name)
        {
            _host.Name = name.ToLower();
            _host.Initialize();
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public ImmateriumHostBuilder UseUser(string username, string password)
        {
            _userPass = new Tuple<string, string>(username, password);
            return this;
        }

        /*
        /// <summary>
        /// 
        /// </summary>
        /// <param name="checker"></param>
        /// <returns></returns>
        public ImmateriumHostBuilder UseHealthChecker(IHealthChecker checker)
        {
            _host.HealthChecker = checker;
            return this;
        }*/

        /// <summary>
        /// 
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public ImmateriumHostBuilder UseOrder(MessageProcessingOrder order)
        {
            _host.ProcessingOrder = order;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ImmateriumHost Build()
        {
            // TODO fix
            ILogger<ImmateriumHostBuilder> logger = null;// TbxLogger.TbxLogger.GetLogger("ImmateriumHostBuilder");

            logger.LogInformation("creating host " + _host.Name);

            logger.LogTrace("using startup");
            var serviceCollection = new ServiceCollection();

            _startup?.ConfigureServices(serviceCollection);
            //serviceCollection.AddLogging(builder => { builder.AddProvider(new NavigatorLoggerProvider()); });
            serviceCollection.AddTransient(services => _host.CreateClient());
            serviceCollection.AddSingleton(new NavigatorClientFactory(_host));

            var pipelineBuilder = new PipelineBuilder();

            //pipelineBuilder.Use(new ExceptionHandlingMiddleware());
            pipelineBuilder.Use(new SerializationMiddleware());
            pipelineBuilder.Use(new MethodsCollectionMiddleware(_host.Name, Assembly.GetCallingAssembly(), serviceCollection));
            _startup?.Configure(pipelineBuilder);
            pipelineBuilder.Use(new DeserializationMiddleware());
            pipelineBuilder.Use(new NavigatorMiddleware());

            pipelineBuilder.Use(async (context, next) =>
            {
                await next();

            });

            if (_userPass != null)
                _host.CreateClient(_userPass.Item1, _userPass.Item2).GetAwaiter().GetResult();

            var container = serviceCollection.BuildServiceProvider();
            var serviceScopeFactory = container.GetRequiredService<IServiceScopeFactory>();

            _host.ServiceScopeFactory = serviceScopeFactory;
            _host.Pipeline = pipelineBuilder.Pipeline;

            logger.LogInformation("Created host " + _host.Name);

            return _host;
        }
    }
}