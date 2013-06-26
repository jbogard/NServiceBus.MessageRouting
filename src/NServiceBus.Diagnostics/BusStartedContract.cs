using System;
using System.Runtime.Serialization;

namespace NServiceBus.Diagnostics
{
    [DataContract]
    public class BusStartedContract : EventArgs
    {
        [DataMember]
        public string Endpoint { get; set; }
    }
}