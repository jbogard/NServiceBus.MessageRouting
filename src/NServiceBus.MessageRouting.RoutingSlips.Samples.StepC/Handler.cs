using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;

namespace NServiceBus.MessageRouting.RoutingSlips.Samples.StepC
{
    public class Handler : IHandleMessages<SequentialProcess>
    {
        public void Handle(SequentialProcess message)
        {
            message.ExcecutedStepC = true;
        }
    }
}