using System;
using System.IO;
using System.ServiceModel;
using NServiceBus.MessageMutator;
using NServiceBus.Serialization;
using Newtonsoft.Json;
using log4net;

namespace NServiceBus.Diagnostics
{
    public class MessageProducer : IMutateIncomingMessages, IMutateOutgoingMessages
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(BusListener));

        public IMessageSerializer Serializer { get; set; }
        private readonly ChannelFactory<IBusListener> _pipeFactory;

        public MessageProducer()
        {
            var uri = "net.tcp://localhost:5050/NServiceBus.Diagnostics";

            //if (!namedPipes.Any(f => f.Contains("NServiceBus.Diagnostics"))) 
            //    return;

            try
            {
                var endpointAddress = new EndpointAddress(uri);
                var binding = new NetTcpBinding();
                _pipeFactory = new ChannelFactory<IBusListener>(binding, endpointAddress);

            }
            catch (EndpointNotFoundException)
            {
            }
        }

        public object MutateIncoming(object message)
        {
            try
            {
                var pipeProxy = _pipeFactory.CreateChannel();
                if (pipeProxy == null || _pipeFactory.State != CommunicationState.Opened)
                {
                    Logger.Warn("Could not publish received message - connection closed.");
                    return message;
                }

                var json = JsonConvert.SerializeObject(message);
                var type = message.GetType().FullName;

                var contract = new MessageReceivedContract
                {
                    Endpoint = Configure.EndpointName,
                    MessageJson = json,
                    MessageType = type
                };
                Logger.Info("Published received message.");
                pipeProxy.MessageReceived(contract);
            }
            catch (EndpointNotFoundException e)
            {
                Logger.Error("Unable to publish received message.", e);
            }
            catch (CommunicationObjectFaultedException e)
            {
                Logger.Error("Unable to publish received message.", e);
            }

            return message;
        }

        public object MutateOutgoing(object message)
        {
            try
            {
                var pipeProxy = _pipeFactory.CreateChannel();
                if (pipeProxy == null || _pipeFactory.State != CommunicationState.Opened)
                {
                    Logger.Warn("Could not publish sent message - connection closed.");
                    return message;
                }

                var json = JsonConvert.SerializeObject(message);
                var contract = new MessageSentContract
                {
                    MessageJson = json,
                    MessageType = message.GetType().FullName
                };
                Logger.Info("Published sent message.");
                pipeProxy.MessageSent(contract);
            }
            catch (EndpointNotFoundException e)
            {
                Logger.Error("Unable to publish received message.", e);
            }
            catch (CommunicationObjectFaultedException e)
            {
                Logger.Error("Unable to publish received message.", e);
            }

            return message;
        }
    }
}