using System.Runtime.Serialization;

namespace NServiceBus.Diagnostics
{
    [DataContract]
    public class BusStartedContract
    {
        [DataMember]
        public string Endpoint { get; set; }
    }
}