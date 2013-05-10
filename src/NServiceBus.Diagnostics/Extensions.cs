namespace NServiceBus.Diagnostics
{
    public static class Extensions
    {
         public static Configure Diagnostics(this Configure configure)
         {
             configure.Configurer.ConfigureComponent<Raiser>(DependencyLifecycle.SingleInstance);
             return configure;
         }
    }
}