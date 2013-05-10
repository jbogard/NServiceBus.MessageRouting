using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using NServiceBus.MessageMutator;
using NServiceBus.Serialization;
using Newtonsoft.Json;

namespace NServiceBus.Diagnostics
{
    [DataContract]
    public class MessageReceivedContract
    {
        [DataMember]
        public string Endpoint { get; set; }

        [DataMember]
        public string MessageJson { get; set; }

        [DataMember]
        public string MessageType { get; set; }
    }

    [DataContract]
    public class MessageSentContract
    {
        [DataMember]
        public string MessageJson { get; set; }

        [DataMember]
        public string MessageType { get; set; }
    }

    [ServiceContract]
    public interface IBusListener
    {
        [OperationContract]
        void MessageReceived(MessageReceivedContract message);

        [OperationContract]
        void MessageSent(MessageSentContract message);
    }

    public class Raiser : IMutateIncomingMessages, IMutateOutgoingMessages
    {
        public IMessageSerializer Serializer { get; set; }
        private readonly IBusListener pipeProxy;
        private readonly ChannelFactory<IBusListener> _pipeFactory;

        public Raiser()
        {
            var uri = "net.pipe://localhost/NServiceBus.Diagnostics";

            var namedPipes = Directory.GetFiles(@"\\.\pipe\");

            //if (!namedPipes.Any(f => f.Contains("NServiceBus.Diagnostics"))) 
            //    return;

            try
            {
                var endpointAddress = new EndpointAddress(uri);
                var binding = new NetNamedPipeBinding();
                _pipeFactory = new ChannelFactory<IBusListener>(binding, endpointAddress);

                pipeProxy = _pipeFactory.CreateChannel();
            }
            catch (EndpointNotFoundException)
            {
            }
        }

        public object MutateIncoming(object message)
        {
            try
            {
                if (pipeProxy == null || _pipeFactory.State != CommunicationState.Opened)
                    return message;

                var json = JsonConvert.SerializeObject(message);
                var type = message.GetType().FullName;

                var contract = new MessageReceivedContract
                {
                    Endpoint = Configure.EndpointName,
                    MessageJson = json,
                    MessageType = type
                };

                pipeProxy.MessageReceived(contract);
            }
            catch (EndpointNotFoundException) { }
            catch (CommunicationObjectFaultedException) { }

            return message;
        }

        public object MutateOutgoing(object message)
        {
            try
            {
                if (pipeProxy == null || _pipeFactory.State != CommunicationState.Opened)
                    return message;

                var json = JsonConvert.SerializeObject(message);
                var contract = new MessageSentContract
                {
                    MessageJson = json,
                    MessageType = message.GetType().FullName
                };
                pipeProxy.MessageSent(contract);
            }
            catch (EndpointNotFoundException) { }
            catch (CommunicationObjectFaultedException) { }

            return message;
        }
    }
}