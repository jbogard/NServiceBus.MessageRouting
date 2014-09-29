namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRouter
    {
        void SendToFirstStep(object message, RoutingSlip routingSlip);
        void SendToNextStep(RoutingSlip routingSlip);
    }
}