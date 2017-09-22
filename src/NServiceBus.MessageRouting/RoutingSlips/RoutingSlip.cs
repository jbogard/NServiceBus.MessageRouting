namespace NServiceBus.MessageRouting.RoutingSlips
{
    using System;
    using System.Collections.Generic;

    public class RoutingSlip
    {
        private RoutingSlip()
        {
            Attachments = new Dictionary<string, string>();
            Itinerary = new List<ProcessingStep>();
            Log = new List<ProcessingStepResult>();
        }

        public RoutingSlip(Guid id, params string[] destinations) : this()
        {
            Id = id;
            destinations = destinations ?? new string[0];
            foreach (var destination in destinations)
            {
                Itinerary.Add(new ProcessingStep { Address = destination });
            }
        }

        public Guid Id { get; }
        public IDictionary<string, string> Attachments { get; }

        public IList<ProcessingStep> Itinerary { get; }
        public IList<ProcessingStepResult> Log { get; }
    }
}