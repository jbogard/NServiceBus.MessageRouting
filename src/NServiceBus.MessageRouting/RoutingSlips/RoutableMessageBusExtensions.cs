using System.Collections;

namespace NServiceBus.MessageRouting.RoutingSlips
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Newtonsoft.Json;

	public static class RoutableMessageBusExtensions
	{
		public static Task Route(this IMessageSession bus, object message, params string[] destinations)
		{
			return bus.Route(message, Guid.NewGuid(), destinations);
		}

		public static Task Route(this IMessageSession bus, object message, IDictionary<string, string> attachments, params string[] destinations)
		{
			return bus.Route(message, Guid.NewGuid(), attachments, destinations);
		}

		public static Task Route(this IMessageSession bus, object message, Guid routingSlipId, params string[] destinations)
		{
			var options = BuildSendOptions(routingSlipId, null, destinations);

			return bus.Send(message, options);
		}

		public static Task Route(this IMessageSession bus, object message, Guid routingSlipId, IDictionary<string, string> attachments, params string[] destinations)
		{
			var options = BuildSendOptions(routingSlipId, attachments, destinations);

			return bus.Send(message, options);
		}

		public static Task Route(this IPipelineContext bus, object message, params string[] destinations)
		{
			return bus.Route(message, Guid.NewGuid(), null, destinations);
		}

		public static Task Route(this IPipelineContext bus, object message, IDictionary<string, string> attachments, params string[] destinations)
		{
			return bus.Route(message, Guid.NewGuid(), attachments, destinations);
		}

		public static Task Route(this IPipelineContext bus, object message, Guid routingSlipId, params string[] destinations)
		{
			var options = BuildSendOptions(routingSlipId, null, destinations);

			return bus.Send(message, options);
		}

		public static Task Route(this IPipelineContext bus, object message, Guid routingSlipId, IDictionary<string, string> attachments, params string[] destinations)
		{
			var options = BuildSendOptions(routingSlipId, attachments, destinations);

			return bus.Send(message, options);
		}

		private static SendOptions BuildSendOptions(Guid routingSlipId, IDictionary<string, string> attachments, string[] destinations)
		{
			var routingSlip = new RoutingSlip(routingSlipId, attachments, destinations);

			var firstRouteDefinition = routingSlip.Itinerary.First();

			var json = JsonConvert.SerializeObject(routingSlip);

			var options = new SendOptions();
			options.SetHeader(Router.RoutingSlipHeaderKey, json);
			options.SetDestination(firstRouteDefinition.Address);
			return options;
		}
	}
}