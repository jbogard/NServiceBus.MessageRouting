using System.ServiceModel;

namespace NServiceBus.Diagnostics
{
    [ServiceContract]
    public interface IBusListener
    {
        [OperationContract(IsOneWay = true)]
        void BusStarted(BusStartedContract message);

        [OperationContract(IsOneWay = true)]
        void MessageReceived(MessageReceivedContract message);

        [OperationContract(IsOneWay = true)]
        void MessageSent(MessageSentContract message);
    }
}