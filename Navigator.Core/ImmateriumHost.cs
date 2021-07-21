using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Immaterium;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Navigator.Builder;
using Navigator.Client;
using Navigator.Pipeline;

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
        internal AppPipeline Pipeline;

        public event EventHandler Started;

        private TaskCompletionSource<bool> _tcs;

        public MessageProcessingOrder ProcessingOrder = MessageProcessingOrder.Parallel;

        /// <summary>
        /// 
        /// </summary>
        internal ImmateriumHost()
        {
            // TODO fix
            ILogger<ImmateriumHost> _logger = null;// TbxLogger.TbxLogger.GetLogger("ImmateriumHost");
            _httpListener = new HttpListener();
        }

        public IServiceScopeFactory ServiceScopeFactory;
        private readonly HttpListener _httpListener;

        /*
        /// <summary>
        /// 
        /// </summary>
        private void HealthCheck()
        {
            _httpListener.Prefixes.Add("http://+:4001/");
            _httpListener.Start();
            _logger.LogInformation("Listening health check on 4001");

            var cts = new CancellationTokenSource();

            Task.Factory.StartNew(async () =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    // from https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=netframework-4.7.2

                    HttpListenerContext context = await _httpListener.GetContextAsync();
                    HttpListenerResponse response = context.Response;
                    response.ContentLength64 = 0;

                    if (HealthChecker != null)
                    {
                        try
                        {
                            response.StatusCode = HealthChecker.HealthCheck() ? 200 : 500;
                        }
                        catch (Exception)
                        {
                            response.StatusCode = 500;
                        }
                    }
                    else
                    {
                        response.StatusCode = 418;
                    }

                    response.Close();
                }
            }, cts.Token);
        }
        */

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
            _imClient = new ImmateruimClient(new LoggerFactory(), new ImmateriumConfig
            {
                ServiceTopic = Name,
                AutoListen = false
            });

            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                _imClient?.Dispose();
                _imClient = null;
            };

            // handle common messages
            _imClient.Received += (sender, e) =>
            {
                if (ProcessingOrder == MessageProcessingOrder.Parallel)
                    Task.Factory.StartNew(async () =>
                    {
                        await HandleMessage(e.ReceivedMessage);
                    });
                else
                    HandleMessage(e.ReceivedMessage).Wait();
            };

            // handle events messages
            _imClient.Event += (sender, e) =>
            {
                if (ProcessingOrder == MessageProcessingOrder.Parallel)
                    Task.Factory.StartNew(async () =>
                    {
                        await HandleEvent(e.ReceivedMessage);
                    });
                else
                    HandleEvent(e.ReceivedMessage).Wait();
            };
        }

        /// <summary>
        /// Starts listening
        /// </summary>
        public void Start()
        {
            _tcs = new TaskCompletionSource<bool>();

            if (HealthChecker != null)
                HealthCheck();

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
                _logger.LogTrace($"send response to: {context.Response.Receiver} with message: {context.Response.Body}");
                await Send(context.Response);
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
            return await _imClient.Send(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="method"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        internal async Task Publish(string method, object body)
        {
            string bodyString = JsonSerializerWrapper.Serialize(new[] { body });
            var message = new ImmateriumMessage
            {
                Type = ImmateriumMessageType.Event,
                Body = bodyString
            };
            message.Headers["Method"] = method;

            await _imClient.Publish(message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serviceName"></param>
        public void Subscribe(string serviceName)
        {
            _imClient.Subscribe(serviceName);
        }

        /// <summary>
        /// Creates NavigatorClient authorized as given user
        /// </summary>
        /// <returns></returns>
        public async Task<NavigatorClient> CreateClient(string username, string password)
        {
            // TODO: fix
            var client = new NavigatorClient(this, null);

            return client;
        }

        /// <summary>
        /// Creates forwarding NavigatorClient
        /// </summary>
        /// <returns></returns>
        public NavigatorClient CreateClient(string jwtToken)
        {
            // TODO: fix
            var client = new NavigatorClient(this, null);
            return client;
        }

        /// <summary>
        /// Creates anonymous client
        /// </summary>
        /// <returns></returns>
        public NavigatorClient CreateClient()
        {
            // TODO: fix
            return new NavigatorClient(this, null);
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