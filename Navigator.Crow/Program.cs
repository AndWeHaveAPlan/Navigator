using System;
using Immaterium.Transports.RabbitMQ;
using Navigator.Core;
using Navigator.Crow.Models;
using RabbitMQ.Client;

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
                    .UseTransport(new RabbitMqTransport())
                    .UseServiceName("crow")
                    .UseOrder(MessageProcessingOrder.Parallel)
                    .Build();
        }
    }
}
