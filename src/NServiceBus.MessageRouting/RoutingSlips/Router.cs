using System;
using System.Linq;
using NServiceBus.Unicast.Queuing;
using NServiceBus.Unicast.Transport;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class Router : IRouter
    {
        public const string RoutingSlipHeaderKey = "NServiceBus.MessageRouting.RoutingSlips.RoutingSlip";

        private readonly IBus _bus;
        private readonly ISendMessages _messageSender;

        public Router(IBus bus, ISendMessages messageSender)
        {
            _bus = bus;
            _messageSender = messageSender;
        }

        public void SendToFirstStep(object message, RoutingSlip routingSlip)
        {
            var firstRouteDefinition = routingSlip.Itinerary.First();

            var json = JsonConvert.SerializeObject(routingSlip);

            _bus.OutgoingHeaders[RoutingSlipHeaderKey] = json;

            _bus.Send(firstRouteDefinition.Address, message);
        }

        public void SendToNextStep(TransportMessage message, Exception ex, RoutingSlip routingSlip)
        {
            if (ex != null)
                return;

            var currentStep = routingSlip.Itinerary.First();
            
            routingSlip.Itinerary.RemoveAt(0);

            var result = new ProcessingStepResult
            {
                Address = currentStep.Address,
            };

            routingSlip.Log.Add(result);

            var nextStep = routingSlip.Itinerary.FirstOrDefault();

            if (nextStep == null)
                return;

            var json = JsonConvert.SerializeObject(routingSlip);

            message.Headers[RoutingSlipHeaderKey] = json;

            var address = Address.Parse(nextStep.Address);

            _messageSender.Send(message, address);
        }
    }
}