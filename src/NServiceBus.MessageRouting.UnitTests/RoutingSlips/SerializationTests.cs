using System;
using NServiceBus.MessageRouting.RoutingSlips;
using NUnit.Framework;
using Should;

namespace NServiceBus.MessageRouting.UnitTests.RoutingSlips
{
    [TestFixture]
    public class SerializationTests
    {
        [Test]
        public void Should_be_able_to_serialize()
        {
            var routingSlip = new RoutingSlip(Guid.NewGuid(), "foo", "bar");
            routingSlip.Log.Add(new ProcessingStepResult { Address = "baz" });

            var result = Serializer.Serialize(routingSlip);

            var deserialized = Serializer.Deserialize<RoutingSlip>(result);

            deserialized.Id.ShouldEqual(routingSlip.Id);
            deserialized.Itinerary.Count.ShouldEqual(2);
            deserialized.Itinerary[0].Address.ShouldEqual("foo");
            deserialized.Itinerary[1].Address.ShouldEqual("bar");
            deserialized.Log.Count.ShouldEqual(1);
            deserialized.Log[0].Address.ShouldEqual("baz");
        }
    }
}