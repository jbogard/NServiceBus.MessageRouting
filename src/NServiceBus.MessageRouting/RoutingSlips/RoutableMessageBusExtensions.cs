using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public static class RoutableMessageBusExtensions
    {
        public static async Task Route(this IBusContext bus, object message, params string[] destinations)
        {
            await bus.Route(message, Guid.NewGuid(), destinations);
        }

        public static async Task Route(this IBusContext bus, object message, Guid routingSlipId, params string[] destinations)
        {
            var routingSlip = new RoutingSlip(routingSlipId, destinations);

            var firstRouteDefinition = routingSlip.Itinerary.First();

            var json = JsonConvert.SerializeObject(routingSlip);

            var options = new SendOptions();
            options.SetHeader(Router.RoutingSlipHeaderKey, json);
            options.SetDestination(firstRouteDefinition.Address);

            await bus.Send(message, options);
        }
    }
}