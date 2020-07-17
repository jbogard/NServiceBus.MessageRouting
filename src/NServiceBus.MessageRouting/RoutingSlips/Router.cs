namespace NServiceBus.MessageRouting.RoutingSlips
{
    using System;
    using System.Text.Json;
    using System.Linq;
    using System.Threading.Tasks;
    using Pipeline;

    public class Router : Behavior<IInvokeHandlerContext>
    {
        public const string RoutingSlipHeaderKey = "NServiceBus.MessageRouting.RoutingSlips.Router";

        public override async Task Invoke(IInvokeHandlerContext context, Func<Task> next)
        {
            if (!context.MessageHeaders.TryGetValue(RoutingSlipHeaderKey, out var routingSlipJson))
            {
                await next().ConfigureAwait(false);
                return;
            }

            var routingSlip = JsonSerializer.Deserialize<RoutingSlip>(routingSlipJson);

            context.Extensions.Set(routingSlip);

            await next().ConfigureAwait(false);

            await SendToNextStep(context, routingSlip).ConfigureAwait(false);
        }

        private static Task SendToNextStep(IInvokeHandlerContext context, RoutingSlip routingSlip)
        {
            routingSlip.RecordStep();

            var nextStep = routingSlip.Itinerary.FirstOrDefault();

            if (nextStep == null)
                return Task.CompletedTask;

            var json = JsonSerializer.Serialize(routingSlip);

            context.Headers[RoutingSlipHeaderKey] = json;

            return context.ForwardCurrentMessageTo(nextStep.Address);
        }
    }
}