![Icon](https://raw.github.com/jbogard/NServiceBus.MessageRouting/master/package_icon.png)

NServiceBus.MessageRouting
==========================

*Implementations of EIP message routing patterns for NServiceBus*

Currently the patterns implemented include the routing slip. Planned are a true saga implementation (w/ compensating actions) and more.

Routing Slips
-------------

The [Routing Slip pattern](http://www.enterpriseintegrationpatterns.com/RoutingTable.html) enables you to route a message to one or more destinations. Each step handles the message and forwards to the next step.

Forwarding is transparent to each handler, nor does each handler need to have any additional configuration for other steps.

To enable in each endpoint, configure routing slips in your EndpointConfiguration:

```c#
configuration.EnableFeature<RoutingSlips>();;
```
    
Then kick off the process by sending a message and including the list of destinations, either from an IPipelineContext inside a message handler or IMessageSession from your endpoint instance:

```c#
// From your endpoint instance
endpoint.Route(order, new[] {"Validate", "Fraud", "CreditCheck", "Process"});

// Inside a message handler
context.Route(order, new[] {"Validate", "Fraud", "CreditCheck", "Process"});
```

Each endpoint needs to include a handler for the message. Optionally, each endpoint can inspect/modify routing slip attachments:

```c#
context.Extensions.Get<RoutingSlip>().Attachments["FraudResult"] = "Declined";
```

Routing slip attachments are data that you share between different handlers in the route steps. As the message flows from step to step, the original raw transport message is passed through as-is.

That's all there is to it!


## Icon 

<a href="http://thenounproject.com/term/Route/41620/" target="_blank">Route</a> designed by <a href="http://thenounproject.com/stevenlester/" target="_blank">Steven Lester</a> from The Noun Project