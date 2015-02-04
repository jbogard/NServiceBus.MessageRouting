using System;
using Newtonsoft.Json;

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
            var routingSlip = new RoutingSlip(routingSlipId, destinations);

            var router = new Router(bus);

            router.SendToFirstStep(message, routingSlip);
        }

        public static RoutingSlip RoutingSlip(this IBus bus)
        {
            string routingSlip;
            if (bus.CurrentMessageContext.Headers.TryGetValue(Router.RoutingSlipHeaderKey, out routingSlip))
                return JsonConvert.DeserializeObject<RoutingSlip>(routingSlip);
            return null;
        }
    }
}