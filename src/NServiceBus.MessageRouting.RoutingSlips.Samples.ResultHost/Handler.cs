﻿using NServiceBus.Logging;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));
        public IBus Bus { get; set; }

        public void Handle(SequentialProcess message)
        {
            var routingSlip = Bus.GetRoutingSlipFromCurrentMessage();

            Logger.Info("Received message for sequential process.");

            foreach (var routeDefinition in routingSlip.Log)
            {
                Logger.Info("Executed step at endpoint " + routeDefinition.Address);
            }
            
            Logger.Info("========================================");
        }
    }
}