using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Installation.Environments;

namespace ServiceHost
{
    class Program
    {
        private static string _workingDir;
        private static readonly List<AppDomain> Services = new List<AppDomain>();
        private static Timer _reloadTimer;
        private static readonly object Sync = new Object();
        private static readonly List<FileSystemWatcher> Watchers = new List<FileSystemWatcher>();
        private const string ShutdownServicehostFilename = "servicehost.shutdown";

        static void Main(string[] args)
        {
            _workingDir = args.Length > 0 && Directory.Exists(args[0])
                                 ? args[0]
                                 : AppDomain.CurrentDomain.BaseDirectory;
            var runInstallers = args.Any(x => x.ToUpperInvariant() == "/INSTALL");
            if (args.Any(x => x.ToUpperInvariant() == "DEBUGBREAK"))
            {
                Debugger.Launch();
            }
            if (args.Contains("/delayload"))
            {
                Console.WriteLine("Sleeping for 10 seconds...");
                Thread.Sleep(TimeSpan.FromSeconds(10));
            }
            LoadServices(runInstallers);
            if (args.Length > 1 && args[1] == "/quickexit")
            {
                Environment.Exit(0);
            }
            Console.WriteLine("\n*** Services Running... Press '.' stop services and exit.");
            while (ProcessKey(Console.ReadKey().KeyChar))
            {
            }
            UnloadServices();
        }

        private static bool ProcessKey(char keyChar)
        {
            switch (keyChar)
            {
                case '.':
                    return false;
                case 'r':
                    Console.WriteLine("Recycling services...");
                    if (_reloadTimer != null)
                    {
                        _reloadTimer.Dispose();
                        _reloadTimer = null;
                    }
                    UnloadServices();
                    LoadServices(false);
                    break;
            }
            return true;
        }

        private static FileSystemWatcher CreateWatcher(string path, string filter)
        {
            var watcher = new FileSystemWatcher
            {
                Path = path,
                IncludeSubdirectories = true,
                Filter = filter,
                NotifyFilter =
                    NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime |
                    NotifyFilters.Size
            };
            watcher.Changed += OnDirectoryChanged;
            watcher.Created += OnDirectoryChanged;
            watcher.Error += OnError;
            watcher.EnableRaisingEvents = true;
            return watcher;
        }

        private static void OnError(object sender, ErrorEventArgs e)
        {
            Console.WriteLine("FilesystemWatcher error {0}", e.GetException().Message);
        }

        private static void OnDirectoryChanged(object sender, FileSystemEventArgs e)
        {
            lock (Sync)
            {
                if (e.Name == ShutdownServicehostFilename)
                {
                    Console.WriteLine("Shutdown signal received!");
                    try
                    {
                        File.Delete(e.FullPath);
                    }
                    catch (IOException)
                    {
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                    Environment.Exit(0);
                }
                if (_reloadTimer == null)
                {
                    Console.WriteLine("Filesystem change detected, queueing restart...");
                    _reloadTimer = new Timer(OnReloadTimerElapsed, null, TimeSpan.FromSeconds(10),
                                             TimeSpan.FromMilliseconds(-1));
                }
            }
        }

        private static void OnReloadTimerElapsed(object state)
        {
            try
            {
                UnloadServices();
                LoadServices(false);
            }
            finally
            {
                _reloadTimer = null;
            }
        }

        private static void UnloadServices()
        {
            Console.WriteLine("Unloading services...");
            foreach (var watcher in Watchers)
            {
                watcher.EnableRaisingEvents = false;
                watcher.Dispose();
            }
            Watchers.Clear();
            Task.WaitAll(Services.Select(domain => Task.Factory.StartNew(() => UnloadAppDomain(domain))).ToArray());
            Services.Clear();
        }

        private static void UnloadAppDomain(AppDomain domain)
        {
            Console.WriteLine("Unloading AppDomain '{0}'", domain.FriendlyName);
            int idx = 1;
            const int maxTries = 10;
            while (true)
            {
                try
                {
                    AppDomain.Unload(domain);
                    break;
                }
                catch (CannotUnloadAppDomainException)
                {
                    Console.WriteLine("Could not unload AppDomain '{0}' at this time.", domain.FriendlyName);
                    if (++idx == maxTries)
                        break;
                    Console.WriteLine("Trying again ({0}/{1})...", idx, maxTries);
                }
            }
        }

        private static void LoadServices(bool runInstallers)
        {
            if (runInstallers)
            {
                Console.WriteLine("Installing NSB infrastructure...");
                Configure.With()
                    .DefaultBuilder()
                    .ForInstallationOn<Windows>().InstallInfrastructureInstallers();
            }
            var serviceDirs = (from d in Directory.GetDirectories(_workingDir)
                               where File.Exists(GetServiceConfig(d))
                               select d).ToArray();

            Console.WriteLine("Attempting to start {0} services...", serviceDirs.Count());
            Watchers.Add(CreateWatcher(_workingDir, ShutdownServicehostFilename));
            var tasks = new List<Task>();
            foreach (var serviceDir in serviceDirs)
            {
                Watchers.Add(CreateWatcher(serviceDir, "*.dll"));
                Watchers.Add(CreateWatcher(serviceDir, "*.config"));

                string applicationName = Path.GetFileName(serviceDir);
                Console.WriteLine("Starting {0}...", applicationName);
                //// Since there is no way to shut down the bus once it is started, we initialize the bus in an external appdomain, which we 
                //// can then unload.            
                var domainInfo = new AppDomainSetup
                {
                    ConfigurationFile = GetServiceConfig(serviceDir),
                    ApplicationBase = serviceDir,
                    ShadowCopyFiles = "true",
                    ApplicationName = applicationName,
                };

                var appDomain = AppDomain.CreateDomain(applicationName ?? String.Empty, null, domainInfo);
                var callback = runInstallers
                                   ? ServiceBusEndpoint.InstallNewInstanceForAppDomain
                                   : (CrossAppDomainDelegate)ServiceBusEndpoint.ConfigureNewInstanceForAppDomain;

                // Initialize in a background thread so we can start up all the app domains simultaneously
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    appDomain.ExecuteAssemblyByName("NServiceBus.Host");
                    Services.Add(appDomain);
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static string GetServiceConfig(string servicePath)
        {
            var dirName = Path.GetFileName(servicePath);
            if (dirName == null)
                throw new InvalidOperationException();
            return Path.Combine(servicePath, dirName + ".dll.config");
        }
    }
}