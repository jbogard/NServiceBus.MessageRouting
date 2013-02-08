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
        private const string RoutingSlipHeaderKey = "NServiceBus.MessageRouting.RoutingSlips.RoutingSlip";

        private readonly IBus _bus;
        private readonly ISendMessages _messageSender;

        public Router(IBus bus, ISendMessages messageSender)
        {
            _bus = bus;
            _messageSender = messageSender;
        }

        void IManageUnitsOfWork.Begin()
        {
        }

        void IManageUnitsOfWork.End(Exception ex)
        {
            if (_currentMessage == null)
                return;

            var routingSlip = GetRoutingSlipFromCurrentMessageInternal();

            if (routingSlip == null)
                return;

            SendToNextStep(_currentMessage, routingSlip, ex);

            _currentMessage = null;
        }

        void IMutateIncomingTransportMessages.MutateIncoming(TransportMessage message)
        {
            _currentMessage = message;
        }

        public void SendToFirstStep(object message, Guid routingSlipId, params string[] destinations)
        {
            var routingSlip = new RoutingSlip(routingSlipId, destinations.Select(d => new RoutingSlip.RouteDefinition(d)).ToArray());

            var firstRouteDefinition = routingSlip.GetFirstUnhandledStep();

            var json = JsonConvert.SerializeObject(routingSlip);

            message.SetHeader(RoutingSlipHeaderKey, json);

            _bus.Send(firstRouteDefinition.Destination, message);
        }

        private void SendToNextStep(TransportMessage message, RoutingSlip routingSlip, Exception ex)
        {
            routingSlip.MarkCurrentStepAsHandled();

            var nextAddress = routingSlip.GetFirstUnhandledStep();

            if (nextAddress == null)
                return;

            if (ex != null)
                return;

            var json = JsonConvert.SerializeObject(routingSlip);

            message.Headers[RoutingSlipHeaderKey] = json;

            var address = Address.Parse(nextAddress.Destination);

            _messageSender.Send(message, address);
        }

        public IRoutingSlip GetRoutingSlipFromCurrentMessage()
        {
            return GetRoutingSlipFromCurrentMessageInternal();
        }

        private RoutingSlip GetRoutingSlipFromCurrentMessageInternal()
        {
            string routingSlipJson;

            if (_bus.CurrentMessageContext.Headers.TryGetValue(RoutingSlipHeaderKey, out routingSlipJson))
            {
                var routingSlip = JsonConvert.DeserializeObject<RoutingSlip>(routingSlipJson);

                return routingSlip;
            }

            return null;
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
            }

            public Guid Id { get; private set; }

            public IReadOnlyList<IRouteDefinition> GetRouteDefintions()
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