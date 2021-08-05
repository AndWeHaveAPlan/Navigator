using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using Immaterium.Transports.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Navigator.Core;
using Navigator.Core.Client;

namespace Navigator.HttpGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var immateriumHost = BuildHost();
            //immateriumHost.Start();
            Assembly.GetEntryAssembly();
            CreateHostBuilder((ImmateriumHost)immateriumHost).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(ImmateriumHost immateriumHost) =>
            Host.CreateDefaultBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureServices(collection =>
                    {
                        collection.AddScoped<NavigatorClient>(provider => immateriumHost.CreateClient());
                    });
                    webBuilder.UseStartup<Startup>();
                });

        public static IHost BuildHost()
        {
            return
                ImmateriumHost.CreateDefaultBuilder()
                    .UseTransport(new RabbitMqTransport())
                    .UseServiceName("http_gateway")
                    .UseOrder(MessageProcessingOrder.Parallel)
                    .Build();
        }
    }
}
