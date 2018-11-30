using System;
using System.Reactive.Linq;

namespace StackExchangeChat.Utilities
{
    public static class EventFilters
    {
        public static IObservable<ChatEvent> OnlyMessages(this IObservable<ChatEvent> observable)
        {
            return observable.Where(c =>
                c.EventDetails.EventType == EventType.MessagePosted ||
                c.EventDetails.EventType == EventType.MessageEdited);
        }
    }
}
