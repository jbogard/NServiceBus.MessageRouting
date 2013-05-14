using System;
using System.Collections.Generic;
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

        public static void Enumerate<T>(this IEnumerable<T> items)
        {
            foreach (var item in items) { }
        }
    }
}