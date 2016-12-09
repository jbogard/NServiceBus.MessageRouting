
namespace NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost
{
    using System;
    using System.Threading.Tasks;
    using Messages;
    using NServiceBus;
    using NServiceBus.Logging;

    class Program
    {
        static void Main()
        {
            RunBus().GetAwaiter().GetResult();
        }

        static async Task RunBus()
        {
            IEndpointInstance endpoint = null;
            try
            {
                var configuration = new EndpointConfiguration("NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost");

                configuration.UseTransport<MsmqTransport>();
                configuration.UsePersistence<InMemoryPersistence>();
                configuration.EnableFeature<RoutingSlips>();

                endpoint = await Endpoint.Start(configuration);

                Console.ReadLine();
            }
            finally
            {
                if (endpoint != null)
                    await endpoint.Stop();
            }
        }
    }
}
