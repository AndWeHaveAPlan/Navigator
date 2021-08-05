using System;
using Immaterium.Transports.RabbitMQ;
using Microsoft.Extensions.Hosting;
using Navigator.Core;
using Navigator.Playground.Models;
using RabbitMQ.Client;

namespace Navigator.Playground2
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
                    .UseServiceName("crow2")
                    .UseOrder(MessageProcessingOrder.Parallel)
                    .Build();


            Console.WriteLine("Hello World!");













            host.Start();

            var client = host.CreateClient();
            //var result = client.Post<SimpleClass>(new NavigatorAddress("crow", "echo", "Object"), 1, "ttt", new SimpleClass { S1 = "sdfs", I1 = 55 }).Result;
            var result = client.Post<SimpleClass>(new NavigatorAddress("crow", "echo", "Object"), 1, "ttt", new SimpleClass() { I1 = 10, S1 = "eee__" }).Result;


            Console.WriteLine("Hello World!");
        }
    }
}