using NServiceBus.Logging;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepA
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));
        public IBus Bus { get; set; }
        public RoutingSlip RoutingSlip { get; set; }

        public void Handle(SequentialProcess message)
        {
            Logger.Info(message.StepAInfo);

            RoutingSlip.Attachments["Foo"] = "Bar";
        }
    }
}