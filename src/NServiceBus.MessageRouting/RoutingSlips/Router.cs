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
            var routingSlip = new RoutingSlip(routingSlipId, destinations.Select(d => new RoutingSlip.ProcessingStep { DestinationAddress = d }).ToArray());

            var firstRouteDefinition = routingSlip.GetFirstUnhandledStep();

            var json = JsonConvert.SerializeObject(routingSlip);

            message.SetHeader(RoutingSlipHeaderKey, json);

            _bus.Send(firstRouteDefinition.DestinationAddress, message);
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

            var address = Address.Parse(nextAddress.DestinationAddress);

            _messageSender.Send(_currentMessage, address);
        }

        private class RoutingSlip : IRoutingSlip
        {
            [JsonProperty("ProcessingSteps")]
            private readonly ProcessingStep[] _processingSteps;

            [JsonConstructor]
            public RoutingSlip(Guid id, ProcessingStep[] processingSteps)
            {
                _processingSteps = processingSteps;
                Id = id;
                Attachments = new Dictionary<string, string>();
            }

            public Guid Id { get; private set; }
            public IDictionary<string, string> Attachments { get; private set; }

            [JsonIgnore]
            public IEnumerable<IProcessingStep> ProcessingSteps
            {
                get { return _processingSteps; }
            }

            public ProcessingStep GetFirstUnhandledStep()
            {
                return _processingSteps.SkipWhile(r => r.Handled).FirstOrDefault();
            }

            public void MarkCurrentStepAsHandled()
            {
                var currentStep = GetFirstUnhandledStep();

                if (currentStep == null)
                    return;

                currentStep.Handled = true;
            }

            public class ProcessingStep : IProcessingStep
            {
                public string DestinationAddress { get; set; }
                public bool Handled { get; set; }
            }
        }
    }
}