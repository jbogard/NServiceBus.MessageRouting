using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;
using log4net;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));

        public void Handle(SequentialProcess message)
        {
            Logger.Info("Received message for sequential process.");
            Logger.Info("Executed Step A: " + message.ExcecutedStepA);
            Logger.Info("Executed Step B: " + message.ExcecutedStepB);
            Logger.Info("Executed Step C: " + message.ExcecutedStepC);
            Logger.Info("========================================");
        }
    }
}