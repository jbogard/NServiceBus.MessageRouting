﻿using NServiceBus.Logging;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepC
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));
        public IBus Bus { get; set; }

        public void Handle(SequentialProcess message)
        {
            Logger.Info(message.StepCInfo);
            var slip = Bus.GetRoutingSlipFromCurrentMessage();

            string fooValue;
            
            if (slip.Attachments.TryGetValue("Foo", out fooValue))
            {
                Logger.Info("Found Foo value of " + fooValue);
            }
        }
    }
}