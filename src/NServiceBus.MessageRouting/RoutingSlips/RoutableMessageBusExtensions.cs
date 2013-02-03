using System.Linq;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public static class RoutableMessageBusExtensions
    {
        public static Configure RoutingSlips(this Configure configure)
        {
            configure.Configurer.ConfigureComponent<Router>(DependencyLifecycle.SingleInstance);

            return configure;
        }

        public static void SendToFirstStep(this IBus bus, object message, params string[] destinations)
        {
            var routeDefinitions = destinations.Select(destination => new RouteDefinition(destination, false)).ToArray();
            
            bus.SendToFirstStep(message, routeDefinitions);
        }

        public static void SendToFirstStep(this IBus bus, object message, params RouteDefinition[] routeDefinitions)
        {
            var routingSlip = new RoutingSlip(routeDefinitions);

            var firstRouteDefinition = routeDefinitions.First();

            var json = JsonConvert.SerializeObject(routingSlip);

            message.SetHeader(Router.RoutingSlipHeaderKey, json);

            bus.Send(firstRouteDefinition.Destination, message);
        }

    }
}