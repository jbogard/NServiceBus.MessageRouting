using System;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRouter
    {
        void SendToFirstStep(object message, RoutingSlip routingSlip);
        void SendToNextStep(Exception ex, RoutingSlip routingSlip);
    }
}