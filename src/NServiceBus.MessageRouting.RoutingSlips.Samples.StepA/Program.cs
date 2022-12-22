
namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepA
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
                var configuration = new EndpointConfiguration("NServiceBus.MessageRouting.RoutingSlips.Samples.StepA");

                configuration.UseTransport<LearningTransport>();
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
