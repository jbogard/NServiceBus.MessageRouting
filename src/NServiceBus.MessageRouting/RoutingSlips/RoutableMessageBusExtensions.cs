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

        public static IRoutingSlip GetRoutingSlipFromCurrentMessage(this IBus bus)
        {
            var router = Configure.Instance.Builder.Build<IRouter>();

            return router.GetRoutingSlip();
        }

        public static void Route(this IBus bus, object message, Guid routingSlipId, params string[] destinations)
        {
            var router = Configure.Instance.Builder.Build<IRouter>();

            router.SendToFirstStep(message, routingSlipId, destinations);
        }
    }
}