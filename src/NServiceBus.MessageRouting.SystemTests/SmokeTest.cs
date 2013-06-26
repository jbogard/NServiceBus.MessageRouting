using System;
using System.Linq;
using NServiceBus.MessageRouting.RoutingSlips;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;
using NUnit.Framework;
using Should;

namespace NServiceBus.MessageRouting.SystemTests
{
    [TestFixture]
    public class SmokeTest
    {
        private BusFixture _fixture;

        [TestFixtureSetUp]
        public void SetUp()
        {
            _fixture = new BusFixture("NServiceBus.MessageRouting.SystemTests");
        }

        [TestFixtureTearDown]
        public void TestFixtureTearDown()
        {
            if (_fixture == null) 
                return;

            try
            {
                _fixture.Dispose();
            }
            finally
            {
                _fixture = null;
            }
        }

        [Test]
        public void Should_route_to_complete_process()
        {
            var messageABC = new SequentialProcess
            {
                StepAInfo = "Foo",
                StepBInfo = "Bar",
                StepCInfo = "Baz",
            };

            var destinations = new[]
            {
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepA",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepB",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepC",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost",
            };

            Console.WriteLine("Sending message...");

            var last = destinations.Last();

            var events = _fixture.SendAndWait(
                bus => bus.Route(messageABC, Guid.NewGuid(), destinations),
                c => c.Endpoint != last,
                TimeSpan.FromSeconds(60)).ToArray();

            events.Count().ShouldEqual(4);
        }

        [Test]
        public void Should_route_to_pared_down_process()
        {
            var messageABC = new SequentialProcess
            {
                StepAInfo = "Foo",
                StepBInfo = "Bar",
                StepCInfo = "Baz",
            };

            var destinations = new[]
            {
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepA",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepC",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost",
            };

            Console.WriteLine("Sending message...");

            var last = destinations.Last();

            var events = _fixture.SendAndWait(
                bus => bus.Route(messageABC, Guid.NewGuid(), destinations),
                c => c.Endpoint != last,
                TimeSpan.FromSeconds(60)).ToArray();

            events.Count().ShouldEqual(3);
        }
    }
}