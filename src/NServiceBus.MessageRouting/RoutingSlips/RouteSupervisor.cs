using System;
using NServiceBus.MessageMutator;
using NServiceBus.Unicast.Transport;
using NServiceBus.UnitOfWork;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    using Pipeline;
    using Pipeline.Contexts;

    public class RouteSupervisor : IBehavior<IncomingContext>
    {
        private readonly IRouter _router;
        public RouteSupervisor(IRouter router)
        {
            _router = router;
        }

        public void Invoke(IncomingContext context, Action next)
        {
            string routingSlipJson;

            if (context.IncomingLogicalMessage.Headers.TryGetValue(Router.RoutingSlipHeaderKey, out routingSlipJson))
            {
                var routingSlip = JsonConvert.DeserializeObject<RoutingSlip>(routingSlipJson);

                context.Set(routingSlip);

                next();

                _router.SendToNextStep(routingSlip);
            }
            else
            {
                next();
            }

        }

        public class Registration : RegisterStep
        {
            public Registration() : base("RoutingSlip", typeof(RouteSupervisor), "Unpacks routing slip and forwards message to next destination")
            {
                InsertBefore(WellKnownStep.LoadHandlers);
            }
        }
    }
}