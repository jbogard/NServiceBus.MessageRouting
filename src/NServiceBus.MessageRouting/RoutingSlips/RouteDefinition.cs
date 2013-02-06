using System;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class RouteDefinition
    {
        public RouteDefinition(string destination)
        {
            Destination = destination;
        }

        public string Destination { get; private set; }
        public bool Handled { get; set; }
    }
}