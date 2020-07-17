using System.ComponentModel;
using System.Linq;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    using System;
    using System.Collections.Generic;

    public class RoutingSlip
    {
        [EditorBrowsable(EditorBrowsableState.Never)]
        public RoutingSlip()
        {
            Attachments = new Dictionary<string, string>();
            Itinerary = new List<ProcessingStep>();
            Log = new List<ProcessingStepResult>();
        }

        public RoutingSlip(Guid id, params string[] destinations) : this()
        {
            Id = id;
            destinations ??= new string[0];
            foreach (var destination in destinations)
            {
                Itinerary.Add(new ProcessingStep { Address = destination });
            }
        }

        public Guid Id { get; set; }
        public IDictionary<string, string> Attachments { get; set; }

        public IList<ProcessingStep> Itinerary { get; set; }
        public IList<ProcessingStepResult> Log { get; set; }

        public void RecordStep()
        {
            var currentStep = Itinerary.First();

            Itinerary.RemoveAt(0);

            var result = new ProcessingStepResult
            {
                Address = currentStep.Address
            };

            Log.Add(result);
        }
    }
}