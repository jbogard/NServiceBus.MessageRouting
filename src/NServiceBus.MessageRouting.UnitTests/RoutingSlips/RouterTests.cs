using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NServiceBus.MessageRouting.RoutingSlips;
using NServiceBus.Testing;
using Xunit;
using Shouldly;

namespace NServiceBus.MessageRouting.UnitTests.RoutingSlips
{
    public class RouterTests
    {
        private class DummyMessage : IMessage
        {

        }

        [Fact]
        public void Should_build_routing_slip()
        {
            var routingSlipId = Guid.NewGuid();

            var routingSlip = new RoutingSlip(routingSlipId, "foo");

            routingSlip.ShouldNotBeNull();
            routingSlip.Id.ShouldBe(routingSlipId);
            routingSlip.Itinerary.Count().ShouldBe(1);
            routingSlip.Itinerary.First().Address.ShouldBe("foo");
        }

        [Fact]
        public async Task Should_send_to_first_destination()
        {
            var bus = new TestableMessageSession();

            var message = new DummyMessage();
            await ((IMessageSession)bus).Route(message, Guid.NewGuid(), "foo");

            bus.SentMessages.Length.ShouldBe(1);

            var sentMessage = bus.SentMessages[0];
            sentMessage.Options.GetHeaders()[Router.RoutingSlipHeaderKey].ShouldNotBeNull();
            sentMessage.Options.GetDestination().ShouldBe("foo");
            sentMessage.Message.ShouldBe(message);
        }

        [Fact]
        public async Task Should_send_to_next_destination_if_no_error()
        {
            var routingSlip = new RoutingSlip(Guid.NewGuid(), "foo", "bar");

            var router = new Router();
            var context = new TestableInvokeHandlerContext
            {
                MessageHeaders =
                {
                    [Router.RoutingSlipHeaderKey] = System.Text.Json.JsonSerializer.Serialize(routingSlip)
                }
            };

            await router.Invoke(context, () => Task.FromResult(0));

            context.Extensions.TryGet(out routingSlip).ShouldBeTrue();

            context.Headers[Router.RoutingSlipHeaderKey].ShouldNotBeNull();

            routingSlip.Itinerary.Count.ShouldBe(1);
            routingSlip.Log.Count.ShouldBe(1);
            routingSlip.Log[0].Address.ShouldBe("foo");

            context.ForwardedMessages.Length.ShouldBe(1);
            context.ForwardedMessages[0].ShouldBe("bar");
        }

        [Fact]
        public async Task Should_complete_route()
        {
            var routingSlip = new RoutingSlip(Guid.NewGuid(), "foo", "bar");
            routingSlip.RecordStep();

            var router = new Router();
            var context = new TestableInvokeHandlerContext
            {
                MessageHeaders =
                {
                    [Router.RoutingSlipHeaderKey] = System.Text.Json.JsonSerializer.Serialize(routingSlip)
                }
            };

            await router.Invoke(context, () => Task.FromResult(0));

            context.Extensions.TryGet(out routingSlip).ShouldBeTrue();

            context.Headers.ContainsKey(Router.RoutingSlipHeaderKey).ShouldBeFalse();

            context.ForwardedMessages.Length.ShouldBe(0);
        }
    }
}