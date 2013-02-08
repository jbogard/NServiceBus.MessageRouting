using System;
using System.Collections.Generic;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRoutingSlip
    {
        IReadOnlyList<IRouteDefinition> GetRouteDefintions();
        Guid Id { get; }
    }
}