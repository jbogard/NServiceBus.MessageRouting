using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.Diagnostics;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;
using NServiceBus.Unicast;
using NUnit.Framework;
using NServiceBus.MessageRouting.RoutingSlips;
using Should;

namespace NServiceBus.MessageRouting.SystemTests
{
    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class Hello : IBusListener
    {
        public static event EventHandler<MessageReceivedContract> MessageReceivedEvent;
        public static event EventHandler<MessageSentContract> MessageSentEvent;
 
        public void MessageReceived(MessageReceivedContract message)
        {
            PrintMessage(message);
            var handler = MessageReceivedEvent;
            if (handler != null)
            {
                handler(this, message);
            }
        }

        public static void PrintMessage(MessageReceivedContract message)
        {
            Console.WriteLine("MessageReceived " + message.Endpoint + ":" + message.MessageType + ":" + message.MessageJson);
        }

        public void MessageSent(MessageSentContract message)
        {
            PrintMessage(message);
            var handler = MessageSentEvent;
            if (handler != null)
            {
                handler(this, message);
            }
        }

        public static void PrintMessage(MessageSentContract message)
        {
            Console.WriteLine("MessageSent " + message.MessageType + ":" + message.MessageJson);
        }
    }

    [TestFixture]
    public class SmokeTest
    {
        private static readonly List<AppDomain> Services = new List<AppDomain>();
        private static Task _nonBlocking;

        [Test]
        public void TestCase()
        {
            //Hello.MessageReceivedEvent += (sender, e) =>
            //{
            //    Console.WriteLine("MessageReceived " + e.Endpoint + ":" + e.MessageType + ":" + e.MessageJson);
            //};
            //Hello.MessageSentEvent += (sender, e) =>
            //{
            //    Console.WriteLine("MessageSent " + e.MessageType + ":" + e.MessageJson);
            //};

            Blorg();
            Blarg();

            var bus = Configure.With()
                .DefineEndpointName("NServiceBus.MessageRouting.SystemTests")
                .DefaultBuilder()
                .RoutingSlips()
                .Diagnostics()
                .XmlSerializer()
                .MsmqTransport()
                .UnicastBus()
                .SendOnly();

            var messageABC = new SequentialProcess
            {
                StepAInfo = "Foo",
                StepBInfo = "Bar",
                StepCInfo = "Baz",
            };

            var destinations = new[]
            {
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepA",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepB",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepC",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost",
            };
            bus.Route(messageABC, Guid.NewGuid(), destinations);

            //Thread.Sleep(60000);
            var rcvd = Observable
                .FromEventPattern<MessageReceivedContract>(typeof(Hello), "MessageReceivedEvent")
                //.TakeWhile(c => c.EventArgs.Endpoint != destinations.Last())
                .TakeUntil(DateTimeOffset.Now.AddSeconds(60))
                ;

            Console.WriteLine("Receiving events...");

            var events = rcvd.ToEnumerable().ToArray();


            foreach (var message in events)
            {
                Hello.PrintMessage(message.EventArgs);
            }

            events.Count().ShouldEqual(4);
        }

        [TearDown]
        public void TearDown()
        {
            Task.WaitAll(Services.Select(domain => Task.Factory.StartNew(() => AppDomain.Unload(domain))).ToArray());
            Services.Clear();
        }

        private void Blorg()
        {
            var source = new CancellationTokenSource();
            var token = source.Token;
            var listener = new Task(() =>
            {
                using (var host = new ServiceHost(typeof (Hello), new[] {new Uri("net.pipe://localhost")}))
                {

                    host.AddServiceEndpoint(typeof(IBusListener), new NetNamedPipeBinding(), "NServiceBus.Diagnostics");

                    host.Open();

                    Console.WriteLine("Service is available. " +
                                      "Press <ENTER> to exit.");

                    while (!token.IsCancellationRequested) { }

                    host.Close();
                }
            }, token);

            listener.Start();
        }

        private static void Blarg()
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
                        //appDomain.
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
    }
}