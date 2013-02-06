namespace NServiceBus.MessageRouting.RoutingSlips.Samples.Messages
{
    public class SequentialProcess : ICommand
    {
        public string StepAInfo { get; set; }
    }
}
