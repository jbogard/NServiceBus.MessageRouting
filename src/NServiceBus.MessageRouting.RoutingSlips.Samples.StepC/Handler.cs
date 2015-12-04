using System.Threading.Tasks;
using NServiceBus.Logging;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepC
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(Handler));

        public Task Handle(SequentialProcess message, IMessageHandlerContext context)
        {
            var routingSlip = context.Extensions.Get<RoutingSlip>();

            Logger.Info(message.StepCInfo);
            string fooValue;

            if (routingSlip.Attachments.TryGetValue("Foo", out fooValue))
            {
                Logger.Info("Found Foo value of " + fooValue);
            }

            return Task.CompletedTask;
        }
    }
}