using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public static class RoutableMessageBusExtensions
    {
        public static Task Route(this IMessageSession bus, object message, params string[] destinations)
        {
            return bus.Route(message, Guid.NewGuid(), destinations);
        }

        public static Task Route(this IMessageSession bus, object message, Guid routingSlipId, params string[] destinations)
        {
            var options = BuildSendOptions(routingSlipId, destinations);

            return bus.Send(message, options);
        }

        public static Task Route(this IPipelineContext bus, object message, params string[] destinations)
        {
            return bus.Route(message, Guid.NewGuid(), destinations);
        }

        public static Task Route(this IPipelineContext bus, object message, Guid routingSlipId, params string[] destinations)
        {
            var options = BuildSendOptions(routingSlipId, destinations);

            return bus.Send(message, options);
        }

        private static SendOptions BuildSendOptions(Guid routingSlipId, string[] destinations)
        {
            var routingSlip = new RoutingSlip(routingSlipId, destinations);

            var firstRouteDefinition = routingSlip.Itinerary.First();

            var json = JsonConvert.SerializeObject(routingSlip);

            var options = new SendOptions();
            options.SetHeader(Router.RoutingSlipHeaderKey, json);
            options.SetDestination(firstRouteDefinition.Address);
            return options;
        }
    }
}