using System;
using System.Collections.Generic;

namespace NServiceBus.MessageRouting.RoutingSlips
{
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

        public Guid Id { get; set; }
        public IDictionary<string, string> Attachments { get; private set; }

        public IList<ProcessingStep> Itinerary { get; private set; }
        public IList<ProcessingStepResult> Log { get; private set; }
    }
}