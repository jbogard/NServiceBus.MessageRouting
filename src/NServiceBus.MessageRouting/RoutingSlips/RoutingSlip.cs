using System.Linq;

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

		public RoutingSlip(Guid id, IDictionary<string, string> attachments, params string[] destinations) : this()
		{
			Id = id;
			destinations = destinations ?? new string[0];
			foreach (var destination in destinations)
			{
				Itinerary.Add(new ProcessingStep { Address = destination });
			}
			if (attachments != null)
				foreach (var attachment in attachments)
				{
					attachments.Add(attachment);
				}
		}
		
		public Guid Id { get; }
		public IDictionary<string, string> Attachments { get; }

		public IList<ProcessingStep> Itinerary { get; }
		public IList<ProcessingStepResult> Log { get; }

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