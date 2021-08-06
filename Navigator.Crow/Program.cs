using System;
using Immaterium.Transports.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Navigator.Core;

namespace Navigator.Crow
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = BuildHost();

            Console.WriteLine("Hello World!");

            //var client = host.CreateClient();

            host.Subscribe("crow", false);

            host.Run();


            //var result = client.Post<SimpleClass>(new NavigatorAddress("crow", "echo", "Object"), 1, "ttt").Result;


            Console.WriteLine("Hello World!");

        }

        public static ImmateriumHost BuildHost()
        {
            return
                ImmateriumHost.CreateDefaultBuilder()
                    .UseTransport(new RabbitMqTransport("amqp://10.20.10.242:5672/"))
                    .UseServiceName("crow")
                    .UseOrder(MessageProcessingOrder.Parallel)
                    .Build();
        }
    }
}
