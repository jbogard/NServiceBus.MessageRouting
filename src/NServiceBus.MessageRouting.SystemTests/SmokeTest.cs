using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus.MessageRouting.RoutingSlips.Samples.Messages;
using NServiceBus.Unicast;
using NUnit.Framework;
using NServiceBus.MessageRouting.RoutingSlips;

namespace NServiceBus.MessageRouting.SystemTests
{
    [TestFixture]
    public class SmokeTest
    {
        private static readonly List<AppDomain> Services = new List<AppDomain>();
        private static Task _nonBlocking;

        [Test]
        public void TestCase()
        {
            Blarg();

            var bus = Configure.With()
                .DefineEndpointName("NServiceBus.MessageRouting.SystemTests")
                .DefaultBuilder()
                .RoutingSlips()
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

            bus.Route(messageABC, Guid.NewGuid(), new[]
            {
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepA",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepB",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.StepC",
                "NServiceBus.MessageRouting.RoutingSlips.Samples.ResultHost",
            });
            Thread.Sleep(10000);

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
                        appDomain.ExecuteAssemblyByName("NServiceBus.Host");
                        //appDomain.
                    }, TaskCreationOptions.LongRunning));
                }
                Task.WaitAll(tasks.ToArray());
            });
            _nonBlocking.Start();
            _nonBlocking.Wait(30000);
            
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