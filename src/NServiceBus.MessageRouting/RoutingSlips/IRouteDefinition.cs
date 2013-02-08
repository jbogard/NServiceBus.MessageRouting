namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRouteDefinition
    {
        string Destination { get; }
        bool Handled { get; }
    }
}