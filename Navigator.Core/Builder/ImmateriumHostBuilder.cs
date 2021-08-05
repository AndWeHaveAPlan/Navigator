using System.Reflection;
using Immaterium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navigator.Core.Client;
using Navigator.Core.Exceptions.Middleware;
using Navigator.Core.Pipeline;
using Navigator.Core.Pipeline.Middleware;

namespace Navigator.Core.Builder
{
    public class ImmateriumHostBuilderOptions
    {
        public IImmateriumTransport ImmateriumTransport;
        public string Name;

        public MessageProcessingOrder ProcessingOrder;
    }

    public class ImmateriumHostBuilder
    {
        private IStartup _startup;

        //private readonly ImmateriumHost _host;

        private readonly ImmateriumHostBuilderOptions _builderOptions = new ImmateriumHostBuilderOptions();

        //private IImmateriumSerializer _immateriumSerializer;
        //private IImmateriumTransport _immateriumTransport;

        /// <summary>
        /// 
        /// </summary>
        internal ImmateriumHostBuilder()
        {
            //_host = new ImmateriumHost();
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
        /// <param name="immateriumTransport"></param>
        /// <returns></returns>
        public ImmateriumHostBuilder UseTransport(IImmateriumTransport immateriumTransport)
        {
            _builderOptions.ImmateriumTransport = immateriumTransport;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        public ImmateriumHostBuilder UseServiceName(string name)
        {
            _builderOptions.Name = name.ToLower();
            //_host.Initialize();
            return this;
        }

        /*
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
        */

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
            _builderOptions.ProcessingOrder = order;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ImmateriumHost Build()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<INavigatorSerializer>(new JsonNavigatorSerializer());
            _startup?.ConfigureServices(serviceCollection);

            serviceCollection.AddLogging(builder =>
            {
                builder.AddSimpleConsole();
                builder.AddDebug();
            });

            var container = serviceCollection.BuildServiceProvider();
            var serviceScopeFactory = container.GetRequiredService<IServiceScopeFactory>();

            using var serviceScope = serviceScopeFactory.CreateScope();

            var host = new ImmateriumHost(
                serviceScope.ServiceProvider.GetRequiredService<ILogger<ImmateriumHost>>(),
                _builderOptions.ImmateriumTransport)
            {
                Name = _builderOptions.Name,
                ProcessingOrder = _builderOptions.ProcessingOrder
            };

            host.Initialize();

            //TODO client factory
            serviceCollection.AddTransient(services => host.CreateClient());
            serviceCollection.AddSingleton(new NavigatorClientFactory(host));

            var pipelineBuilder = new PipelineBuilder();

            pipelineBuilder.Use(new ExceptionHandlingMiddleware());
            pipelineBuilder.Use(new SerializationMiddleware());
            pipelineBuilder.Use(new MethodsCollectionMiddleware(Assembly.GetCallingAssembly(), serviceCollection));
            _startup?.Configure(pipelineBuilder);
            pipelineBuilder.Use(new DeserializationMiddleware());
            pipelineBuilder.Use(new NavigatorMiddleware());

            pipelineBuilder.Use(async (context, next) =>
            {
                await next();
            });

            container = serviceCollection.BuildServiceProvider();
            serviceScopeFactory = container.GetRequiredService<IServiceScopeFactory>();

            host.Services = container;
            host.ServiceScopeFactory = serviceScopeFactory;
            host.Pipeline = pipelineBuilder.Pipeline;

            //logger.LogInformation("Created host " + host.Name);

            return host;
        }
    }
}