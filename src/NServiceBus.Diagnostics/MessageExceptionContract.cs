using System;
using System.Runtime.Serialization;

namespace NServiceBus.Diagnostics
{
    [DataContract]
    public class MessageExceptionContract : EventArgs
    {
        [DataMember]
        public string Endpoint { get; set; }
        
        [DataMember]
        public string MessageJson { get; set; }

        [DataMember]
        public string MessageType { get; set; }

        [DataMember]
        public string Exception { get; set; }
    }
}