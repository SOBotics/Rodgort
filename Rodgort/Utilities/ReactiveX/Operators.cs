using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rodgort.Utilities.ReactiveX
{
    public static class Operators
    {
        public static IObservable<IList<TSource>> SlidingBuffer<TSource>(this IObservable<TSource> source, TimeSpan timeSpan)
        {
            return source.Buffer(() =>
            {
                return source.Select(a => Observable.Timer(timeSpan)).Switch();
            });
        }
    }
}
