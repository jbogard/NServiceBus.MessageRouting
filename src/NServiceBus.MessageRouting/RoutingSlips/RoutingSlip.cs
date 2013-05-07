using System;
using System.Collections.Generic;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public class RoutingSlip
    {
        public RoutingSlip()
        {
            Attachments = new Dictionary<string, string>();
            Itinerary = new List<ProcessingStep>();
            Log = new List<ProcessingStepResult>();
        }

        public Guid Id { get; set; }
        public IDictionary<string, string> Attachments { get; private set; }

        public IList<ProcessingStep> Itinerary { get; private set; }
        public IList<ProcessingStepResult> Log { get; private set; }
    }
}