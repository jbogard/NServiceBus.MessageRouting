namespace NServiceBus.MessageRouting.RoutingSlips.Samples.Messages
{
    public class SequentialProcess : ICommand
    {
        public bool ExcecutedStepA { get; set; }
        public bool ExcecutedStepB { get; set; }
        public bool ExcecutedStepC { get; set; }
    }
}
