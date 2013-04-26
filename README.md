NServiceBus.MessageRouting
==========================

*Implementations of EIP message routing patterns for NServiceBus*

Currently the patterns implemented include the routing slip. Planned are a true saga implementation (w/ compensating actions) and more.

Routing Slips
-------------

The [Routing Slip pattern](http://www.enterpriseintegrationpatterns.com/RoutingTable.html) enables you to route a message to one or more destinations. Each step handles the message and forwards to the next step.

Forwarding is transparent to each handler, nor does each handler need to have any additional configuration for other steps.

To enable in each endpoint, configure routing slips:

    Configure.Instance.RoutingSlips();
    
Then kick off the process by sending a message and including the list of destinations:

    Bus.Route(order, new[] {"Validate", "Fraud", "CreditCheck", "Process"});
    
Each endpoint needs to include a handler for the message. Optionally, each endpoint can inspect/modify routing slip attachments:

    Bus.GetRoutingSlipFromCurrentMessage().Attachments["FraudResult"] = "Declined";
    
That's all there is to it!
