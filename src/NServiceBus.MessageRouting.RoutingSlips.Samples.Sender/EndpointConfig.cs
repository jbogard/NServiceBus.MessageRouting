using System;
using NServiceBus.Logging;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.Sender 
{
    using NServiceBus;

	/*
		This class configures this endpoint as a Server. More information about how to configure the NServiceBus host
		can be found here: http://nservicebus.com/GenericHost.aspx
	*/
    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public void Customize(BusConfiguration configuration)
        {
            configuration.RoutingSlips();
            configuration.UsePersistence<InMemoryPersistence>();
        }
    }

    public class Startup : IWantToRunWhenBusStartsAndStops
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Startup));

        public IBus Bus { get; set; }

        public void Start()
        {
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
                    Bus.Route(messageABC, Guid.NewGuid(), new[]
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
                    Bus.Route(messageAC, Guid.NewGuid(), new[]
                    {
                        "NServiceBus.MessageRouting.RoutingSlips.Samples.StepA",
                        "NServiceBus.MessageRouting.RoutingSlips.Samples.StepC",
                        "NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost",
                    });
                }

                toggle = !toggle;
            }
        }

        public void Stop()
        {

        }
    }

}