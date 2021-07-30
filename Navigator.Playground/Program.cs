using System;
using Immaterium.Transports.RabbitMQ;
using Navigator.Core;
using Navigator.Playground.Models;
using RabbitMQ.Client;

namespace Navigator.Playground
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { };
            using var connection = factory.CreateConnection();

            var host =
                ImmateriumHost.CreateDefaultBuilder()
                    .UseTransport(new RabbitMqTransport(connection))
                    .UseServiceName("crow")
                    .UseOrder(MessageProcessingOrder.Parallel)
                    .Build();


            Console.WriteLine("Hello World!");

            var client = host.CreateClient();

            host.Run();


            var result = client.Post<SimpleClass>(new NavigatorAddress("crow", "echo", "Object"), 1, "ttt").Result;


            Console.WriteLine("Hello World!");

        }
    }
}
