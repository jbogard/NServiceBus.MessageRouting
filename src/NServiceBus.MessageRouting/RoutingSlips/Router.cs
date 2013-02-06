using System;
using System.Linq;
using NServiceBus.MessageMutator;
using NServiceBus.Unicast.Queuing;
using NServiceBus.Unicast.Transport;
using NServiceBus.UnitOfWork;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class Router : IManageUnitsOfWork, IMutateIncomingTransportMessages
    {
        [ThreadStatic]
        private static TransportMessage _currentMessage;
        public static readonly string RoutingSlipHeaderKey = "NServiceBus.MessageRouting.RoutingSlips.RoutingSlip";

        public IBus Bus { get; set; }
        public ISendMessages MessageSender { get; set; }

        public void Begin()
        {
        }

        public void End(Exception ex = null)
        {
            if (_currentMessage == null)
                return;

            var routingSlip = Bus.GetRoutingSlipFromCurrentMessage();

            if (routingSlip == null)
                return;

            SendToNextStep(_currentMessage, routingSlip, ex);

            _currentMessage = null;
        }

        public void MutateIncoming(TransportMessage message)
        {
            _currentMessage = message;
        }

        public void SendToFirstStep(object message, RoutingSlip routingSlip)
        {
            var firstRouteDefinition = routingSlip.RouteDefintions.First();

            var json = JsonConvert.SerializeObject(routingSlip);

            message.SetHeader(RoutingSlipHeaderKey, json);

            Bus.Send(firstRouteDefinition.Destination, message);
        }

        public void SendToNextStep(TransportMessage message, RoutingSlip routingSlip, Exception ex)
        {
            routingSlip.MarkCurrentStepAsHandled(ex);

            var nextAddress = routingSlip.GetFirstUnhandledStep();

            if (nextAddress == null)
                return;

            if (ex != null)
                return;

            var json = JsonConvert.SerializeObject(routingSlip);

            message.Headers[RoutingSlipHeaderKey] = json;

            var address = Address.Parse(nextAddress.Destination);

            MessageSender.Send(message, address);
        }
    }
}