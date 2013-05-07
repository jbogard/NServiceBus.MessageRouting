using System;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class RoutingSlipBuilder : IRoutingSlipBuilder
    {
        public RoutingSlip CreateRoutingSlip(Guid routingSlipId, params string[] destinations)
        {
            var routingSlip = new RoutingSlip { Id = routingSlipId };
            foreach (var destination in destinations)
            {
                routingSlip.Itinerary.Add(new ProcessingStep { Address = destination});
            }
            return routingSlip;
        }
    }
}