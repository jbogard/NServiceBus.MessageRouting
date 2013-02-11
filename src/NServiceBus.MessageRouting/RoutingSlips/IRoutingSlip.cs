using System;
using System.Collections.Generic;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRoutingSlip
    {
        IReadOnlyList<IRouteDefinition> GetRouteDefinitions();
        Guid Id { get; }
        IDictionary<string, string> Values { get; }
    }
}