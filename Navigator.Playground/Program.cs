using System;
using Immaterium.Transports.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Navigator.Core;
using RabbitMQ.Client;

namespace Navigator.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { };
            using var connection = factory.CreateConnection();

            var host = BuildHost(connection);

            Console.WriteLine("Hello World!");

            //var client = host.CreateClient();

            host.Run();


            //var result = client.Post<SimpleClass>(new NavigatorAddress("crow", "echo", "Object"), 1, "ttt").Result;


            Console.WriteLine("Hello World!");

        }

        public static ImmateriumHost BuildHost(IConnection connection)
        {
            //var factory = new ConnectionFactory() { };
            //using var connection = factory.CreateConnection();

            return
                ImmateriumHost.CreateDefaultBuilder()
                    .UseTransport(new RabbitMqTransport(connection))
                    .UseServiceName("crow")
                    .UseOrder(MessageProcessingOrder.Parallel)
                    .Build();
        }
    }
}
