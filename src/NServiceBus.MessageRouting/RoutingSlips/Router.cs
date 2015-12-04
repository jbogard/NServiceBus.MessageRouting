using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NServiceBus.Features;
using NServiceBus.Pipeline;
using NServiceBus.Pipeline.Contexts;
using NServiceBus.Transports;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class Router : Behavior<InvokeHandlerContext>
    {
        public const string RoutingSlipHeaderKey = "NServiceBus.MessageRouting.RoutingSlips.Router";

        public override async Task Invoke(InvokeHandlerContext context, Func<Task> next)
        {
            string routingSlipJson;

            if (!context.MessageHeaders.TryGetValue(RoutingSlipHeaderKey, out routingSlipJson))
            {
                await next().ConfigureAwait(false);
                return;
            }

            var routingSlip = JsonConvert.DeserializeObject<RoutingSlip>(routingSlipJson);

            context.Set(routingSlip);

            await next().ConfigureAwait(false);

            await SendToNextStep(context, routingSlip).ConfigureAwait(false);
        }

        private static async Task SendToNextStep(IncomingContext context, RoutingSlip routingSlip)
        {
            var currentStep = routingSlip.Itinerary.First();

            routingSlip.Itinerary.RemoveAt(0);

            var result = new ProcessingStepResult
            {
                Address = currentStep.Address
            };

            routingSlip.Log.Add(result);

            var nextStep = routingSlip.Itinerary.FirstOrDefault();

            if (nextStep == null)
                return;

            var json = JsonConvert.SerializeObject(routingSlip);

            var messageBeingProcessed = context.Get<IncomingMessage>();
            messageBeingProcessed.Headers[RoutingSlipHeaderKey] = json;

            await context.ForwardCurrentMessageTo(nextStep.Address);
        }

        public class Registration : RegisterStep
        {
            public Registration()
                : base(
                    "RoutingSlipBehavior", typeof (Router),
                    "Unpacks routing slip and forwards message to next destination")
            {
                //InsertBefore(WellKnownStep.MutateIncomingTransportMessage);
            }
        }
    }

    public class RoutingSlips : Feature
    {
        public RoutingSlips()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Pipeline.Register("RoutingSlipBehavior", typeof(Router), "Unpacks routing slip and forwards message to next destination");
        }
    }
}