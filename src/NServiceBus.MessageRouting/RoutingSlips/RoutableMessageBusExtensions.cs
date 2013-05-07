using System;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public static class RoutableMessageBusExtensions
    {
        public static Configure RoutingSlips(this Configure configure)
        {
            configure.Configurer.ConfigureComponent<Router>(DependencyLifecycle.SingleInstance);
            configure.Configurer.ConfigureComponent<RouteSupervisor>(DependencyLifecycle.SingleInstance);
            configure.Configurer.ConfigureComponent<RoutingSlipBuilder>(DependencyLifecycle.SingleInstance);

            return configure;
        }

        public static RoutingSlip GetRoutingSlipFromCurrentMessage(this IBus bus)
        {
            return Configure.Instance.Builder.Build<IRouteSupervisor>().RoutingSlip;
        }

        public static void Route(this IBus bus, object message, params string[] destinations)
        {
            bus.Route(message, Guid.NewGuid(), destinations);
        }

        public static void Route(this IBus bus, object message, Guid routingSlipId, params string[] destinations)
        {
            var builder = new RoutingSlipBuilder();
            
            var routingSlip = builder.CreateRoutingSlip(routingSlipId, destinations);

            var router = Configure.Instance.Builder.Build<IRouter>();

            router.SendToFirstStep(message, routingSlip);
        }
    }
}