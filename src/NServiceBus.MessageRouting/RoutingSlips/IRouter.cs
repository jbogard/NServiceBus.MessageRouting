using System;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRouter
    {
        IRoutingSlip GetRoutingSlip();

        void SendToFirstStep(object message, Guid routingSlipId, params string[] destinations);
    }
}