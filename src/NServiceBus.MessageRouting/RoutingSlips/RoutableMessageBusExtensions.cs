using System;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    using Pipeline;

    public static class RoutableMessageBusExtensions
    {
        public static BusConfiguration RoutingSlips(this BusConfiguration configure)
        {
            configure.RegisterComponents(cfg =>
            {
                cfg.ConfigureComponent<Router>(DependencyLifecycle.SingleInstance);
                cfg.ConfigureComponent(b => b.Build<PipelineExecutor>().CurrentContext.Get<RoutingSlip>(), DependencyLifecycle.InstancePerCall);
                cfg.ConfigureComponent<RoutingSlipBuilder>(DependencyLifecycle.SingleInstance);
            });
            configure.Pipeline.Register<RouteSupervisor.Registration>();

            return configure;
        }

        public static void Route(this IBus bus, object message, params string[] destinations)
        {
            bus.Route(message, Guid.NewGuid(), destinations);
        }

        public static void Route(this IBus bus, object message, Guid routingSlipId, params string[] destinations)
        {
            var builder = new RoutingSlipBuilder();
            
            var routingSlip = builder.CreateRoutingSlip(routingSlipId, destinations);

            var router = new Router(bus);

            router.SendToFirstStep(message, routingSlip);
        }
    }
}