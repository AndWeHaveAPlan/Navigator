using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Immaterium.Transports.RabbitMQ;
using Microsoft.Extensions.DependencyInjection;
using Navigator.Core;
using Navigator.Core.Client;
using RabbitMQ.Client;

namespace Navigator.HttpGateway
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var immateriumHost = BuildHost();
            //immateriumHost.Start();
            Assembly.GetEntryAssembly();
            CreateHostBuilder(immateriumHost).Build().Run();
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

        public static ImmateriumHost BuildHost()
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
