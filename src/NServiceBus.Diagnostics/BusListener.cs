using System;
using System.ServiceModel;

namespace NServiceBus.Diagnostics
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true, InstanceContextMode = InstanceContextMode.Single)]
    public class BusListener : IBusListener
    {
        public static event EventHandler<MessageReceivedContract> MessageReceivedEvent = (s, e) => { };
        public static event EventHandler<MessageSentContract> MessageSentEvent = (s, e) => { };
        public static event EventHandler<BusStartedContract> BusStartedEvent = (s, e) => { };
        public static event EventHandler<MessageExceptionContract> MessageExceptionEvent = (s, e) => { };

        public void BusStarted(BusStartedContract message)
        {
            BusStartedEvent(this, message);
        }

        public void MessageReceived(MessageReceivedContract message)
        {
            MessageReceivedEvent(this, message);
        }

        public void MessageSent(MessageSentContract message)
        {
            MessageSentEvent(this, message);
        }

        public void MessageException(MessageExceptionContract message)
        {
            MessageExceptionEvent(this, message);
        }
    }
}