namespace NServiceBus.Diagnostics
{
    public static class Extensions
    {
         public static Configure Diagnostics(this Configure configure)
         {
             configure.Configurer.ConfigureComponent<MessageProducer>(DependencyLifecycle.SingleInstance);
             return configure;
         }
    }
}