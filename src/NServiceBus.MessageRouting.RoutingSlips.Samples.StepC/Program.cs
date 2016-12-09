
namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepC
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus;

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
                var configuration = new EndpointConfiguration("NServiceBus.MessageRouting.RoutingSlips.Samples.StepC");

                configuration.UseTransport<MsmqTransport>();
                configuration.UsePersistence<InMemoryPersistence>();
                configuration.EnableFeature<RoutingSlips>();
                configuration.SendFailedMessagesTo("error");

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
