using System;
using System.Reactive.Linq;

namespace NServiceBus.MessageRouting.SystemTests
{
    public static class ObservableExtensions
    {
        public static IObservable<T> TakeWhileInclusive<T>(
            this IObservable<T> source, Func<T, bool> predicate)
        {
            return source.Publish(co => co.TakeWhile(predicate)
                                          .Merge(co.SkipWhile(predicate).Take(1)));
        }
    }
}