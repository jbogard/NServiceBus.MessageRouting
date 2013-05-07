using System;
using NServiceBus.MessageMutator;
using NServiceBus.Unicast.Transport;
using NServiceBus.UnitOfWork;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRouteSupervisor
    {
        RoutingSlip RoutingSlip { get; }
    }

    public class RouteSupervisor : IManageUnitsOfWork, IMutateIncomingTransportMessages, IRouteSupervisor
    {
        private readonly IRouter _router;
        private readonly IBus _bus;

        [ThreadStatic]
        private static TransportMessage _currentMessage;
        [ThreadStatic]
        private static RoutingSlip _routingSlip;

        public RoutingSlip RoutingSlip { get { return _routingSlip; } }

        public RouteSupervisor(IRouter router, IBus bus)
        {
            _router = router;
            _bus = bus;
        }

        public void Begin()
        {
        }

        public void End(Exception ex)
        {
            if (_currentMessage == null)
                return;

            if (_routingSlip == null)
                return;

            try
            {
                _router.SendToNextStep(_currentMessage, ex, _routingSlip);
            }
            finally
            {
                _currentMessage = null;
                _routingSlip = null;
            }
        }

        public void MutateIncoming(TransportMessage message)
        {
            _currentMessage = message;

            string routingSlipJson;

            if (_bus.CurrentMessageContext.Headers.TryGetValue(Router.RoutingSlipHeaderKey, out routingSlipJson))
            {
                _routingSlip = JsonConvert.DeserializeObject<RoutingSlip>(routingSlipJson);
            }
        }
    }
}