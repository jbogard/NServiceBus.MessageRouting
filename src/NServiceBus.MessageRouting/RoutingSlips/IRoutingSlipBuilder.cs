using System;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRoutingSlipBuilder 
    {
        RoutingSlip CreateRoutingSlip(Guid routingSlipId, params string[] destinations);
    }
}