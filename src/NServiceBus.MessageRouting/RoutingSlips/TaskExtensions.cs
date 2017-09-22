namespace NServiceBus.MessageRouting.RoutingSlips
{
    using System.Threading.Tasks;

    static class TaskExtensions
    {
        public static Task CompletedTask = Task.FromResult(0);
    }
}