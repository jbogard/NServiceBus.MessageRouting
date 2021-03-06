﻿using System;
using NServiceBus.MessageRouting.RoutingSlips;
using Xunit;
using Shouldly;

namespace NServiceBus.MessageRouting.UnitTests.RoutingSlips
{
    public class SerializationTests
    {
        [Fact]
        public void Should_be_able_to_serialize()
        {
            var routingSlip = new RoutingSlip(Guid.NewGuid(), "foo", "bar");
            routingSlip.Log.Add(new ProcessingStepResult { Address = "baz" });

            var result = System.Text.Json.JsonSerializer.Serialize(routingSlip);

            var deserialized = System.Text.Json.JsonSerializer.Deserialize<RoutingSlip>(result);

            deserialized.Id.ShouldBe(routingSlip.Id);
            deserialized.Itinerary.Count.ShouldBe(2);
            deserialized.Itinerary[0].Address.ShouldBe("foo");
            deserialized.Itinerary[1].Address.ShouldBe("bar");
            deserialized.Log.Count.ShouldBe(1);
            deserialized.Log[0].Address.ShouldBe("baz");
        }
    }
}