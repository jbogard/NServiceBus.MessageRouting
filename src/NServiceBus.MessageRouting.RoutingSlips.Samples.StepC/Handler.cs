using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;
using log4net;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepC
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));

        public void Handle(SequentialProcess message)
        {
            Logger.Info(message.StepCInfo);
        }
    }
}