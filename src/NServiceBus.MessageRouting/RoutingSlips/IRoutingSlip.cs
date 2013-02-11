using System;
using System.Collections.Generic;

namespace NServiceBus.MessageRouting.RoutingSlips
{
    public interface IRoutingSlip
    {
        Guid Id { get; }
        IEnumerable<IProcessingStep> ProcessingSteps { get; }
        IDictionary<string, string> Attachments { get; }
    }
}