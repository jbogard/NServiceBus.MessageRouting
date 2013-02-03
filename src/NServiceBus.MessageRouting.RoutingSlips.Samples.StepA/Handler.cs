using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepA
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        public void Handle(SequentialProcess message)
        {
            message.ExcecutedStepA = true;
        }
    }
}