namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class RoutingSlip
    {
        public RoutingSlip(RouteDefinition[] routeDefintions)
        {
            RouteDefintions = routeDefintions;
        }

        public RouteDefinition[] RouteDefintions { get; private set; }
    }
}