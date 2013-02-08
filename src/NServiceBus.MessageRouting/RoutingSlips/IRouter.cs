using System;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRouter
    {
        IRoutingSlip GetRoutingSlipFromCurrentMessage();
        void SendToFirstStep(object message, Guid routingSlipId, params string[] destinations);
    }
}