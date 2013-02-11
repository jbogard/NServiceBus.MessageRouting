using System;
using System.Collections.Generic;
using System.Linq;
using NServiceBus.MessageMutator;
using NServiceBus.Unicast.Queuing;
using NServiceBus.Unicast.Transport;
using NServiceBus.UnitOfWork;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class Router : IRouter, IManageUnitsOfWork, IMutateIncomingTransportMessages
    {
        [ThreadStatic]
        private static TransportMessage _currentMessage;
        [ThreadStatic]
        private static RoutingSlip _routingSlip;

        private const string RoutingSlipHeaderKey = "NServiceBus.MessageRouting.RoutingSlips.RoutingSlip";

        private readonly IBus _bus;
        private readonly ISendMessages _messageSender;

        public Router(IBus bus, ISendMessages messageSender)
        {
            _bus = bus;
            _messageSender = messageSender;
        }

        public IRoutingSlip GetRoutingSlip()
        {
            return _routingSlip;
        }

        void IManageUnitsOfWork.Begin()
        {
        }

        void IManageUnitsOfWork.End(Exception ex)
        {
            if (_currentMessage == null)
                return;

            if (_routingSlip == null)
                return;

            try
            {
                SendToNextStep(ex);
            }
            finally
            {
                _currentMessage = null;
                _routingSlip = null;
            }
        }

        void IMutateIncomingTransportMessages.MutateIncoming(TransportMessage message)
        {
            _currentMessage = message;

            string routingSlipJson;

            if (_bus.CurrentMessageContext.Headers.TryGetValue(RoutingSlipHeaderKey, out routingSlipJson))
            {
                _routingSlip = JsonConvert.DeserializeObject<RoutingSlip>(routingSlipJson);
            }
        }

        public void SendToFirstStep(object message, Guid routingSlipId, params string[] destinations)
        {
            var routingSlip = new RoutingSlip(routingSlipId, destinations.Select(d => new RoutingSlip.RouteDefinition(d)).ToArray());

            var firstRouteDefinition = routingSlip.GetFirstUnhandledStep();

            var json = JsonConvert.SerializeObject(routingSlip);

            message.SetHeader(RoutingSlipHeaderKey, json);

            _bus.Send(firstRouteDefinition.Destination, message);
        }

        private void SendToNextStep(Exception ex)
        {
            _routingSlip.MarkCurrentStepAsHandled();

            var nextAddress = _routingSlip.GetFirstUnhandledStep();

            if (nextAddress == null)
                return;

            if (ex != null)
                return;

            var json = JsonConvert.SerializeObject(_routingSlip);

            _currentMessage.Headers[RoutingSlipHeaderKey] = json;

            var address = Address.Parse(nextAddress.Destination);

            _messageSender.Send(_currentMessage, address);
        }

        private class RoutingSlip : IRoutingSlip
        {
            [JsonProperty]
            private readonly RouteDefinition[] _routeDefinitions;

            [JsonConstructor]
            public RoutingSlip(Guid id, RouteDefinition[] routeDefinitions)
            {
                _routeDefinitions = routeDefinitions;
                Id = id;
                Values = new Dictionary<string, string>();
            }

            public Guid Id { get; private set; }
            public IDictionary<string, string> Values { get; private set; }

            public IReadOnlyList<IRouteDefinition> GetRouteDefinitions()
            {
                return _routeDefinitions;
            }

            public RouteDefinition GetFirstUnhandledStep()
            {
                return _routeDefinitions.SkipWhile(r => r.Handled).FirstOrDefault();
            }

            public void MarkCurrentStepAsHandled()
            {
                var currentStep = GetFirstUnhandledStep();

                if (currentStep == null)
                    return;

                currentStep.Handled = true;
            }

            public class RouteDefinition : IRouteDefinition
            {
                public RouteDefinition(string destination)
                {
                    Destination = destination;
                }

                public string Destination { get; private set; }
                public bool Handled { get; set; }
            }
        }
    }
}