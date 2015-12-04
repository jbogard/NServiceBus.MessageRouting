using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));

        public Task Handle(SequentialProcess message, IMessageHandlerContext context)
        {
            var routingSlip = context.Extensions.Get<RoutingSlip>();

            Logger.Info("Received message for sequential process.");

            foreach (var routeDefinition in routingSlip.Log)
            {
                Logger.Info("Executed step at endpoint " + routeDefinition.Address);
            }

            Logger.Info("======================================== blarg");

            return Task.CompletedTask;
        }
    }
}