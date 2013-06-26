using System;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace NServiceBus.Diagnostics
{

    public class Listener
    {
        private CancellationTokenSource _source;
        private Task _listenerTask;
        private static readonly BusListener _listener = new BusListener();

        public void Start()
        {
            _source = new CancellationTokenSource();
            var token = _source.Token;
            _listenerTask = new Task(() =>
            {
                BusListener.MessageReceivedEvent += (s, e) => Console.WriteLine("Received message at " + e.Endpoint + " of type " + e.MessageType);
                BusListener.MessageSentEvent += (s, e) => Console.WriteLine("Sent message " + e.MessageType);
                BusListener.BusStartedEvent += (s, e) => Console.WriteLine("Bus started " + e.Endpoint);
                BusListener.MessageExceptionEvent += (s, e) => Console.WriteLine("Exception with message " + e.Endpoint + " for type " + e.MessageType + " with value " + e.Exception);
                using (var host = new ServiceHost(_listener, new[] { new Uri("net.tcp://localhost:5050") }))
                {

                    host.AddServiceEndpoint(typeof(IBusListener), new NetTcpBinding(), "NServiceBus.Diagnostics");

                    host.Opened += (s, e) => Console.WriteLine("Listening for events...");
                    host.Closed += (s, e) => Console.WriteLine("Closed listening for events...");

                    host.Open();

                    while (!token.IsCancellationRequested) { }

                    host.Close();
                }
            }, token);

            _listenerTask.Start();
            _listenerTask.Wait(10000);

        }
        public void Stop()
        {
            if (_source != null)
            {
                _source.Cancel();

                Task.WaitAll(_listenerTask);
            }
        }
    }
}