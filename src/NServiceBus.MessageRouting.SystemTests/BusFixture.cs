using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NServiceBus.Diagnostics;
using NServiceBus.MessageRouting.RoutingSlips;

namespace NServiceBus.MessageRouting.SystemTests
{
    public class BusFixture : IDisposable
    {
        private readonly List<AppDomain> Services = new List<AppDomain>();
        private Task _nonBlocking;
        private bool _disposed;
        private Listener _listener;
        private IBus _bus;

        public BusFixture(string endpointName)
        {
            InitializeListener();
            RunHosts();
            InitializeBus(endpointName);
        }

        public IEnumerable<MessageReceivedContract> SendAndWait(Action<IBus> busAction, Func<MessageReceivedContract, bool> predicate,
                                                            TimeSpan timeout)
        {

            var rcvd = Observable
                .FromEventPattern<MessageReceivedContract>(typeof(BusListener), "MessageReceivedEvent", Scheduler.Immediate)
                .Select(e => e.EventArgs)
                .TakeWhileInclusive(predicate)
                .TakeUntil(Observable.Timer(timeout))
                .Publish();

            rcvd.Subscribe();
            rcvd.Connect();

            Console.WriteLine("Executing bus action...");

            busAction(_bus);

            return rcvd.ToEnumerable();
        }

        private void InitializeBus(string endpointName)
        {
            _bus = Configure
                .With()
                .DefineEndpointName(endpointName)
                .DefaultBuilder()
                .RoutingSlips()
                .Diagnostics()
                .XmlSerializer()
                .MsmqTransport()
                .UnicastBus()
                .SendOnly();
        }

        private void InitializeListener()
        {
            _listener = new Listener();
            _listener.Start();
        }

        private void RunHosts()
        {
            var sourceRootDir = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));

            var serviceDirs = (from d in Directory.GetDirectories(sourceRootDir)
                               let bin = Path.Combine(d, "bin", "Debug")
                               where File.Exists(GetServiceConfig(bin))
                               where File.Exists(Path.Combine(bin, "NServiceBus.Host.exe"))
                               select bin).ToArray();

            Console.WriteLine("Attempting to start {0} services...", serviceDirs.Count());

            _nonBlocking = new Task(() =>
            {
                var tasks = new List<Task>();
                foreach (var serviceDir in serviceDirs)
                {
                    string applicationName = GetServiceName(serviceDir);
                    Console.WriteLine("Starting {0}...", applicationName);
                    //// Since there is no way to shut down the bus once it is started, we initialize the bus in an external appdomain, which we 
                    //// can then unload.            
                    var domainInfo = new AppDomainSetup
                    {
                        ConfigurationFile = GetServiceConfig(serviceDir),
                        ApplicationBase = serviceDir,
                        PrivateBinPath = serviceDir,
                        ShadowCopyFiles = "true",
                        ApplicationName = applicationName,

                    };


                    var appDomain = AppDomain.CreateDomain(applicationName
                                                           ?? String.Empty, null, domainInfo);

                    // Initialize in a background thread so we can start up all the app domains simultaneously
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        Services.Add(appDomain);
                        try
                        {
                            appDomain.ExecuteAssemblyByName("NServiceBus.Host");
                        }
                        catch (AppDomainUnloadedException)
                        {
                        }
                    }, TaskCreationOptions.LongRunning));
                }
                Task.WaitAll(tasks.ToArray());
            });
            _nonBlocking.Start();
            _nonBlocking.Wait(45000);
        }

        private static string GetServiceName(string servicePath)
        {
            return Path.GetFileName(Path.GetFullPath(Path.Combine(servicePath, "..", "..")));
        }

        private static string GetServiceConfig(string servicePath)
        {
            var dirName = GetServiceName(servicePath);
            if (dirName == null)
                throw new InvalidOperationException();
            return Path.Combine(servicePath, dirName + ".dll.config");
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Task.WaitAll(Services.Select(domain => Task.Factory.StartNew(() => AppDomain.Unload(domain))).ToArray());
                    _listener.Stop();
                }

                Services.Clear();
                _disposed = true;
            }
        }
    }
}