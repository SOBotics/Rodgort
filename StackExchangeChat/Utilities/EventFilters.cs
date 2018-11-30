using System;
using System.Linq;
using System.Reactive.Linq;

namespace StackExchangeChat.Utilities
{
    public static class EventFilters
    {
        public static IObservable<ChatEvent> OnlyMessages(this IObservable<ChatEvent> observable)
        {
            return observable.OnlyEventTypes(ChatEventType.MessagePosted, ChatEventType.MessageEdited);
        }

        public static IObservable<ChatEvent> Pinged(this IObservable<ChatEvent> observable)
        {
            return observable.OnlyEventTypes(ChatEventType.MessageReply, ChatEventType.UserMentioned);
        }

        /// <summary>
        /// If a user pings the bot from another room, an event will still be emitted in the subscribed room.
        /// This filters messages to only those posted in the room subscribed to.
        /// </summary>
        public static IObservable<ChatEvent> SameRoomOnly(this IObservable<ChatEvent> observable)
        {
            return observable.Where(c => c.ChatEventDetails.RoomId == c.RoomDetails.RoomId);
        }

        public static IObservable<ChatEvent> OnlyEventTypes(this IObservable<ChatEvent> observable, params ChatEventType[] chatEventTypes)
        {
            return observable.Where(c => chatEventTypes.Contains(c.ChatEventDetails.ChatEventType));
        }
    }
}
