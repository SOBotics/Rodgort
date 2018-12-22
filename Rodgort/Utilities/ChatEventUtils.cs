using System;
using StackExchangeChat;
using StackExchangeChat.Utilities;

namespace Rodgort.Utilities
{
    public static class ChatEventUtils
    {
        public static IObservable<ChatEvent> ReplyAlive(this IObservable<ChatEvent> observable)
        {
            observable
                .OnlyMessages()
                .SameRoomOnly()
                .Subscribe(async o =>
            {
                var lookingFor = $"@{o.RoomDetails.MyUserName} alive";
                if (string.Equals(o.ChatEventDetails.Content, lookingFor))
                {
                    await o.ChatClient.SendMessage(o.RoomDetails.ChatSite, o.RoomDetails.RoomId, $":{o.ChatEventDetails.MessageId} yep");
                }
            });
            return observable;
        }
    }
}
