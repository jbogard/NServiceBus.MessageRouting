
namespace NServiceBus.MessageRouting.RoutingSlips.Samples.Sender
{
    using System;
    using System.Threading.Tasks;
    using Messages;
    using NServiceBus;
    using Logging;

    class Program
    {

        private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

        static void Main()
        {
            RunBus().GetAwaiter().GetResult();
        }

        static async Task RunBus()
        {
            IEndpointInstance endpoint = null;
            try
            {
                var configuration = new EndpointConfiguration("NServiceBus.MessageRouting.RoutingSlips.Samples.Sender");

                configuration.UseTransport<MsmqTransport>();
                configuration.UsePersistence<InMemoryPersistence>();
                configuration.EnableFeature<RoutingSlips>();
                configuration.SendFailedMessagesTo("error");

                endpoint = await Endpoint.Start(configuration);


                var toggle = false;

                while (Console.ReadLine() != null)
                {
                    if (toggle)
                    {
                        var messageABC = new SequentialProcess
                        {
                            StepAInfo = "Foo",
                            StepBInfo = "Bar",
                            StepCInfo = "Baz",
                        };

                        Logger.Info("Sending message for step A, B, C");
                        await endpoint.Route(messageABC, Guid.NewGuid(), new[]
                        {
                            "NServiceBus.MessageRouting.RoutingSlips.Samples.StepA",
                            "NServiceBus.MessageRouting.RoutingSlips.Samples.StepB",
                            "NServiceBus.MessageRouting.RoutingSlips.Samples.StepC",
                            "NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost",
                        });
                    }
                    else
                    {
                        var messageAC = new SequentialProcess
                        {
                            StepAInfo = "Foo",
                            StepCInfo = "Baz",
                        };

                        Logger.Info("Sending message for step A, C");
                        await endpoint.Route(messageAC, Guid.NewGuid(), new[]
                        {
                            "NServiceBus.MessageRouting.RoutingSlips.Samples.StepA",
                            "NServiceBus.MessageRouting.RoutingSlips.Samples.StepC",
                            "NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost",
                        });
                    }

                    toggle = !toggle;
                }
            }
            finally
            {
                await endpoint.Stop();
            }
        }
    }
}
