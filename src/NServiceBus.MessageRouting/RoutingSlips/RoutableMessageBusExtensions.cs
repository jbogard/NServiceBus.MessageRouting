using System;
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

        public static RoutingSlip GetRoutingSlipFromCurrentMessage(this IBus bus)
        {
            string routingSlipJson;
            if (bus.CurrentMessageContext.Headers.TryGetValue(Router.RoutingSlipHeaderKey, out routingSlipJson))
            {
                var routingSlip = JsonConvert.DeserializeObject<RoutingSlip>(routingSlipJson);

                return routingSlip;
            }

            return null;
        }


        public static void Route(this IBus bus, object message, Guid routingSlipId, params string[] destinations)
        {
            var routeDefinitions = destinations.Select(destination => new RouteDefinition(destination)).ToArray();

            var routingSlip = new RoutingSlip(routingSlipId, routeDefinitions);

            var router = Configure.Instance.Builder.Build<Router>();

            router.SendToFirstStep(message, routingSlip);
        }
    }
}