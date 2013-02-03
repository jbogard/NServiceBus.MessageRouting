using System;
using System.Linq;
using NServiceBus.MessageMutator;
using NServiceBus.UnitOfWork;
using Newtonsoft.Json;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class Router : IManageUnitsOfWork, IMutateIncomingMessages
    {
        [ThreadStatic] private static object _currentMessage;
        public static readonly string RoutingSlipHeaderKey = "NServiceBus.MessageRouting.RoutingSlips.RoutingSlip";

        public IBus Bus { get; set; }

        public void Begin()
        {
        }

        public void End(Exception ex = null)
        {
            if (_currentMessage != null)
            {
                string routingSlipJson;

                if (Bus.CurrentMessageContext.Headers.TryGetValue(RoutingSlipHeaderKey, out routingSlipJson))
                {
                    var routingSlip = JsonConvert.DeserializeObject<RoutingSlip>(routingSlipJson);

                    SendToNextStep(_currentMessage, routingSlip, ex);
                }
            }
            _currentMessage = null;
        }

        public object MutateIncoming(object message)
        {
            _currentMessage = message;
            return message;
        }

        private void SendToNextStep(object message, RoutingSlip routingSlip, Exception ex)
        {
            MarkCurrentStepAsHandled(routingSlip);

            var nextAddress = GetNextUnhandledStep(routingSlip);

            if (nextAddress == null)
                return;

            if (ex != null && !nextAddress.ContinueOnError)
                return;

            var json = JsonConvert.SerializeObject(routingSlip);

            message.SetHeader(RoutingSlipHeaderKey, json);

            Bus.Send(nextAddress.Destination, message);
        }

        private static void MarkCurrentStepAsHandled(RoutingSlip routingSlip)
        {
            var nextUnhandledStep = GetNextUnhandledStep(routingSlip);

            if (nextUnhandledStep == null)
                return;

            nextUnhandledStep.Handled = true;
        }

        private static RouteDefinition GetNextUnhandledStep(RoutingSlip routingSlip)
        {
            return routingSlip.RouteDefintions.SkipWhile(r => r.Handled).FirstOrDefault();
        }

    }
}