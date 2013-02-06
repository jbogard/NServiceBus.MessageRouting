using System;
using System.Linq;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class RoutingSlip
    {
        public RoutingSlip(Guid id, RouteDefinition[] routeDefintions)
        {
            RouteDefintions = routeDefintions;
            Id = id;
        }


        public Guid Id { get; private set; }

        public RouteDefinition[] RouteDefintions { get; private set; }

        public RouteDefinition GetFirstUnhandledStep()
        {
            return RouteDefintions.SkipWhile(r => r.Handled).FirstOrDefault();
        }

        public RouteDefinition GetLastHandledStep()
        {
            return RouteDefintions.LastOrDefault(r => r.Handled);
        }

        public void MarkCurrentStepAsHandled()
        {
            var currentStep = GetFirstUnhandledStep();

            if (currentStep == null)
                return;

            currentStep.Handled = true;
        }
    }
}