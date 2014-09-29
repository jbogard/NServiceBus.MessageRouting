using NServiceBus.Logging;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepC
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));
        public IBus Bus { get; set; }
        public RoutingSlip RoutingSlip { get; set; }

        public void Handle(SequentialProcess message)
        {
            Logger.Info(message.StepCInfo);
            string fooValue;
            
            if (RoutingSlip.Attachments.TryGetValue("Foo", out fooValue))
            {
                Logger.Info("Found Foo value of " + fooValue);
            }
        }
    }
}