using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Immaterium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navigator.Core.Builder;
using Navigator.Core.Client;
using Navigator.Core.Pipeline;
using Navigator.Core.Pipeline.Middleware;

namespace Navigator.Core
{
    public enum MessageProcessingOrder
    {
        Parallel,
        Sequential
    }

    /// <summary>
    /// 
    /// </summary>
    public sealed class ImmateriumHost : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// 
        /// </summary>
        private ImmateruimClient _imClient;
        private readonly ILogger _logger;
        private readonly IImmateriumTransport _immateriumTransport;
        internal AppPipeline Pipeline;

        public event EventHandler Started;

        private TaskCompletionSource<bool> _tcs;

        public MessageProcessingOrder ProcessingOrder = MessageProcessingOrder.Parallel;

        /// <summary>
        /// 
        /// </summary>
        internal ImmateriumHost(ILogger<ImmateriumHost> logger, IImmateriumTransport immateriumTransport)
        {
            _logger = logger;
            _immateriumTransport = immateriumTransport;
            _httpListener = new HttpListener();
        }

        public IServiceScopeFactory ServiceScopeFactory;
        public ServiceProvider ServiceProvider;
        private readonly HttpListener _httpListener;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ImmateriumHostBuilder CreateDefaultBuilder()
        {
            return new ImmateriumHostBuilder();
        }

        /// <summary>
        /// /
        /// </summary>
        public void Initialize()
        {
            // TODO fix
            _imClient = new ImmateruimClient(Name, _immateriumTransport);

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                _imClient?.Dispose();
                _imClient = null;
            };

            // handle common messages
            _imClient.OnMessage += (sender, e) =>
            {
                if (ProcessingOrder == MessageProcessingOrder.Parallel)
                    Task.Factory.StartNew(async () =>
                    {
                        await HandleMessage(e.Message);
                    });
                else
                    HandleMessage(e.Message).Wait();
            };

            // handle events messages
            /*_imClient. += (sender, e) =>
            {
                if (ProcessingOrder == MessageProcessingOrder.Parallel)
                    Task.Factory.StartNew(async () =>
                    {
                        await HandleEvent(e.ReceivedMessage);
                    });
                else
                    HandleEvent(e.ReceivedMessage).Wait();
            };*/
        }

        /// <summary>
        /// Starts listening
        /// </summary>
        public void Start()
        {
            _tcs = new TaskCompletionSource<bool>();

            _imClient.Listen();
            Started?.Invoke(this, null);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task HandleMessage(ImmateriumMessage immateriumMessage)
        {
            NavigatorContext context = new NavigatorContext(immateriumMessage);

            try
            {
                _logger.LogTrace($"received message from: {immateriumMessage.Headers["Sender"]} to: {immateriumMessage.Receiver} with message: {immateriumMessage.Body}");
                await ProcessPipeline(context);
                _logger.LogTrace("handled");
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception in message pipeline processing");
            }

            if (context.ResponseRequired)
            {
                _logger.LogTrace($"send response to: {context.Response.RawMessage.Receiver} with message: {context.Response.RawMessage.Body}");
                await Send(context.Response.RawMessage);
            }
        }

        private async Task HandleEvent(ImmateriumMessage immateriumMessage)
        {
            NavigatorContext context = new NavigatorContext(immateriumMessage) { IsEvent = true };

            try
            {
                await ProcessPipeline(context);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unhandled exception in event pipeline processing");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ProcessPipeline(NavigatorContext context)
        {
            using (IServiceScope scope = ServiceScopeFactory.CreateScope())
            {
                IServiceProvider serviceProvider = scope.ServiceProvider;
                context.ServiceProvider = serviceProvider;

                await Pipeline.Run(context);
                //Services.AddLogging()
            }
        }

        /// <summary>
        /// Runs a host and block the calling thread until host shutdown.
        /// </summary>
        public void Run()
        {
            Start();
            _tcs.Task.GetAwaiter().GetResult();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        internal async Task<ImmateriumMessage> Send(ImmateriumMessage message)
        {
            try
            {

                if (message.Headers.Type == ImmateriumMessageType.Request)
                    return await _imClient.PostRaw(message);
                else
                {
                    _imClient.SendRaw(message);
                    return null;
                }
            }
            catch (Exception e)
            {
                //TODO: remove console
                Console.WriteLine(e);
                throw;
            }
        }

        internal async Task<ImmateriumMessage> Send(ImmateriumHeaderCollection headers, object body)
        {
            try
            {
                var serializer = ServiceProvider.GetServices<INavigatorSerializer>().First();
                var message = new ImmateriumMessage(headers);
                message.Body = serializer.CreateBody(body);

                if (headers.Type == ImmateriumMessageType.Request)
                    return await _imClient.PostRaw(message);
                else
                {
                    _imClient.SendRaw(message);
                    return null;
                }
            }
            catch (Exception e)
            {
                //TODO: remove console
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        internal async Task Publish(string method, params object[] body)
        {
            var serializer = ServiceProvider.GetServices<INavigatorSerializer>().First();

            var bytes = serializer.CreateBody(body);

            //string bodyString = JsonSerializerWrapper.Serialize(new[] { body });
            var message = new ImmateriumMessage
            {
                Type = ImmateriumMessageType.Event,
                Body = bytes
            };
            message.Headers["Method"] = method;

            _imClient.Publish(bytes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        /// <param name="durable"></param>
        public void Subscribe(string serviceName, bool durable = true)
        {
            var subscriber = new Subscriber(message =>
            {
                //TODO: logic
            });

            _imClient.SubscribeRaw(serviceName, subscriber, durable);
        }

        /// <summary>
        /// Creates anonymous client
        /// </summary>
        /// <returns></returns>
        public NavigatorClient CreateClient()
        {
            return new NavigatorClient(this, null, ServiceProvider.GetServices<INavigatorSerializer>());
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _httpListener?.Stop();
            _imClient?.Dispose();
            _imClient = null;
        }
    }
}