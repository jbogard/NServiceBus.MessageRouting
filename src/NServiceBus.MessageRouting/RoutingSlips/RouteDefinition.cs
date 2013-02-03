namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class RouteDefinition
    {
        public RouteDefinition(string destination, bool continueOnError)
        {
            Destination = destination;
            ContinueOnError = continueOnError;
        }

        public string Destination { get; private set; }
        public bool ContinueOnError { get; private set; }
        public bool Handled { get; set; }
    }
}