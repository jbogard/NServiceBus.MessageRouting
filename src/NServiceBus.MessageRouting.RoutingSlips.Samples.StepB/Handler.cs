using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepB
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        public void Handle(SequentialProcess message)
        {
            message.ExcecutedStepB = true;
        }
    }
}