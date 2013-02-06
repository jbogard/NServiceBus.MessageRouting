namespace NServiceBus.MessageRouting.RoutingSlips.Samples.Messages
{
    public class SequentialProcess : ICommand
    {
        public string StepCInfo { get; set; }
    }
}
