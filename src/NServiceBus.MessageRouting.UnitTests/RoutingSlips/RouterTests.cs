using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using NServiceBus.MessageRouting.RoutingSlips;
using NServiceBus.Unicast.Queuing;
using NServiceBus.Unicast.Transport;
using NUnit.Framework;
using Should;

namespace NServiceBus.MessageRouting.UnitTests.RoutingSlips
{
    [TestFixture]
    public class RouterTests 
    {
        private class DummyMessage : IMessage
        {
            
        }

        [Test]
        public void Should_build_routing_slip()
        {
            var builder = new RoutingSlipBuilder();

            var routingSlipId = Guid.NewGuid();

            RoutingSlip routingSlip = builder.CreateRoutingSlip(routingSlipId, "foo");

            routingSlip.ShouldNotBeNull();
            routingSlip.Id.ShouldEqual(routingSlipId);
            routingSlip.Itinerary.Count().ShouldEqual(1);
            routingSlip.Itinerary.First().Address.ShouldEqual("foo");
        }

        [Test]
        public void Should_send_to_first_destination()
        {
            var routingSlip = new RoutingSlipBuilder().CreateRoutingSlip(Guid.NewGuid(), "foo");

            var bus = new Bus();
            var router = new Router(bus, null);

            var message = new DummyMessage();
            router.SendToFirstStep(message, routingSlip);

            bus.OutgoingHeaders[Router.RoutingSlipHeaderKey].ShouldNotBeNull();
            bus.ExplicitlySent.Count().ShouldEqual(1);
            bus.ExplicitlySent.First().Item1.ShouldEqual("foo");
            bus.ExplicitlySent.First().Item2.ShouldEqual(message);
        }

        [Test]
        public void Should_send_to_next_destination_if_no_error()
        {
            var routingSlip = new RoutingSlipBuilder().CreateRoutingSlip(Guid.NewGuid(), "foo", "bar");

            var bus = new Bus();
            var sender = A.Fake<ISendMessages>();
            var router = new Router(bus, sender);

            var message = new TransportMessage
            {
                Headers = new Dictionary<string, string>()
            };
            router.SendToNextStep(message, null, routingSlip);

            message.Headers[Router.RoutingSlipHeaderKey].ShouldNotBeNull();

            routingSlip.Itinerary.Count.ShouldEqual(1);
            routingSlip.Log.Count.ShouldEqual(1);
            routingSlip.Log[0].Address.ShouldEqual("foo");

            var nextDestination = Address.Parse("bar");
            A.CallTo(() => sender.Send(message, nextDestination)).MustHaveHappened();


        }

        [Test]
        public void Should_complete_route()
        {
            var routingSlip = new RoutingSlipBuilder().CreateRoutingSlip(Guid.NewGuid(), "foo", "bar");

            var bus = new Bus();
            var sender = A.Fake<ISendMessages>();
            var router = new Router(bus, sender);

            var message = new TransportMessage
            {
                Headers = new Dictionary<string, string>()
            };
            router.SendToNextStep(message, null, routingSlip);
            
            message.Headers.Clear();
            
            router.SendToNextStep(message, null, routingSlip);

            message.Headers.ContainsKey(Router.RoutingSlipHeaderKey).ShouldBeFalse();

            routingSlip.Itinerary.Count.ShouldEqual(0);
            routingSlip.Log.Count.ShouldEqual(2);
            routingSlip.Log[0].Address.ShouldEqual("foo");

            A.CallTo(() => sender.Send(message, null)).WithAnyArguments().MustHaveHappened(Repeated.NoMoreThan.Once);


        }


        [Test]
        public void Should_not_send_to_next_destination_if_error()
        {
            var routingSlip = new RoutingSlipBuilder().CreateRoutingSlip(Guid.NewGuid(), "foo", "bar");

            var bus = new Bus();
            var sender = A.Fake<ISendMessages>();
            var router = new Router(bus, sender);

            var message = new TransportMessage
            {
                Headers = new Dictionary<string, string>()
            };
            router.SendToNextStep(message, new Exception(), routingSlip);

            message.Headers.ContainsKey(Router.RoutingSlipHeaderKey).ShouldBeFalse();

            routingSlip.Itinerary.Count.ShouldEqual(2);
            routingSlip.Log.Count.ShouldEqual(0);

            var nextDestination = Address.Parse("bar");
            A.CallTo(() => sender.Send(message, nextDestination)).MustNotHaveHappened();
        }
    }
}