// This class is actually in a different assembly & namespace fro ServiceHost in my solution, 
// you may or may not need that.

using NServiceBus;

namespace ServiceHost
{
    public class ServiceBusEndpoint
    {
        public static void InstallNewInstanceForAppDomain()
        {
            
        }
        public static void ConfigureNewInstanceForAppDomain()
        {
            // Call NSB configuration
            Configure.With();
        }
    }
}