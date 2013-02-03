using NServiceBus.MessageRouting.RoutingSlips;
using NServiceBus.Serializers.Json;
using NUnit.Framework;
using Should;

namespace NServiceBus.MessageRouting.UnitTests.RoutingSlips
{
    [TestFixture]
    public class SerializationTests
    {
        private RoutingSlip _deserialized;
        private RoutingSlip _routingSlip;

        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            _routingSlip = new RoutingSlip(new[]
            {
                new RouteDefinition("foo", true),
                new RouteDefinition("bar", false),
                new RouteDefinition("bazz", true),
            });
            _routingSlip.RouteDefintions[2].Handled = true;

            var serializer = new JsonMessageSerializer(null);

            var json = serializer.SerializeObject(_routingSlip);

            _deserialized = serializer.DeserializeObject<RoutingSlip>(json);
        }

        [Test]
        public void Should_create_all_route_definitions()
        {
            _deserialized.RouteDefintions.Length.ShouldEqual(_routingSlip.RouteDefintions.Length);
        }

        [Test]
        public void Should_serialize_route_destinations()
        {
            for (int i = 0; i < _routingSlip.RouteDefintions.Length; i++)
            {
                _deserialized.RouteDefintions[i].Destination.ShouldEqual(_routingSlip.RouteDefintions[i].Destination);
                _deserialized.RouteDefintions[i].ContinueOnError.ShouldEqual(_routingSlip.RouteDefintions[i].ContinueOnError);
                _deserialized.RouteDefintions[i].Handled.ShouldEqual(_routingSlip.RouteDefintions[i].Handled);
            }
        }
    }
}