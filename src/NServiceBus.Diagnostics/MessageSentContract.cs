using System;
using System.Runtime.Serialization;

namespace NServiceBus.Diagnostics
{
    [DataContract]
    public class MessageSentContract : EventArgs
    {
        [DataMember]
        public string MessageJson { get; set; }

        [DataMember]
        public string MessageType { get; set; }
    }
}