using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navigator.Client;
using Navigator.Core;
//using Navigator.Exceptions.Middleware;
using Navigator.Pipeline;
using Navigator.Pipeline.Middleware;
using System.Reflection;
using Immaterium;

namespace Navigator.Builder
{
    public class ImmateriumHostBuilderOptions
    {
        public IImmateriumSerializer _immateriumSerializer;
        public IImmateriumTransport _immateriumTransport;
        public string Name;

        public MessageProcessingOrder ProcessingOrder;
    }

    public class ImmateriumHostBuilder
    {
        private IStartup _startup;

        //private readonly ImmateriumHost _host;

        private ImmateriumHostBuilderOptions _builderOptions = new ImmateriumHostBuilderOptions();

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
            _builderOptions._immateriumTransport = immateriumTransport;
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="immateriumSerializer"></param>
        /// <returns></returns>
        public ImmateriumHostBuilder UseSerializer(IImmateriumSerializer immateriumSerializer)
        {
            _builderOptions._immateriumSerializer = immateriumSerializer;
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
            _startup?.ConfigureServices(serviceCollection);

            //serviceCollection.a
            //ConsoleLogger

            var container = serviceCollection.BuildServiceProvider();
            var serviceScopeFactory = container.GetRequiredService<IServiceScopeFactory>();

            using var serviceScope = serviceScopeFactory.CreateScope();

            var host = new ImmateriumHost(
                serviceScope.ServiceProvider.GetRequiredService<ILogger<ImmateriumHost>>(),
                _builderOptions._immateriumSerializer,
                _builderOptions._immateriumTransport)
            {
                Name = _builderOptions.Name,
                ProcessingOrder = _builderOptions.ProcessingOrder
            };


            //logger.LogInformation("creating host " + host.Name);

            //serviceCollection.AddLogging(builder => { builder.AddProvider(new NavigatorLoggerProvider()); });

            //TODO client factory
            serviceCollection.AddTransient(services => host.CreateClient());
            serviceCollection.AddSingleton(new NavigatorClientFactory(host));

            var pipelineBuilder = new PipelineBuilder();

            //pipelineBuilder.Use(new ExceptionHandlingMiddleware());
            pipelineBuilder.Use(new SerializationMiddleware());
            pipelineBuilder.Use(new MethodsCollectionMiddleware(host.Name, Assembly.GetCallingAssembly(), serviceCollection));
            _startup?.Configure(pipelineBuilder);
            pipelineBuilder.Use(new DeserializationMiddleware());
            pipelineBuilder.Use(new NavigatorMiddleware());

            pipelineBuilder.Use(async (context, next) =>
            {
                await next();
            });

            host.ServiceScopeFactory = serviceScopeFactory;
            host.Pipeline = pipelineBuilder.Pipeline;

            //logger.LogInformation("Created host " + host.Name);

            return host;
        }
    }
}