namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IProcessingStep
    {
        string DestinationAddress { get; }
        bool Handled { get; }
    }
}