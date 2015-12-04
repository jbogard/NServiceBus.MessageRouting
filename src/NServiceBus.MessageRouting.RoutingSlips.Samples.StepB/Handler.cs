using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepB
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));

        public Task Handle(SequentialProcess message, IMessageHandlerContext context)
        {
            Logger.Info(message.StepBInfo);

            return Task.CompletedTask;
        }
    }
}