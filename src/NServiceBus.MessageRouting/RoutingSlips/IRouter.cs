using System;
using NServiceBus.Unicast.Transport;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRouter
    {
        void SendToFirstStep(object message, RoutingSlip routingSlip);
        void SendToNextStep(TransportMessage message, Exception ex, RoutingSlip routingSlip);
    }
}