using System.Threading.Tasks;
using Rodgort.Utilities;
using StackExchangeChat;

namespace Rodgort.Services
{
    public class NewBurninationService
    {
        private readonly ChatClient _chatClient;
        public NewBurninationService(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task CreateRoomForBurn(ChatSite chatSite, int currentRoomId, string tag, string metaPostUrl)
        {
            var roomName = $"Observation room for [{tag}] burnination";
            var roomId = await _chatClient.CreateRoom(chatSite, currentRoomId, roomName, string.Empty);

            var gemmyMessage = $"@Gemmy start tag [{tag}] {roomId} https://chat.stackoverflow.com/rooms/{roomId}";

            await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, gemmyMessage);

            var burninationMessage = $"The burnination of [tag:{tag}] has begun! {metaPostUrl}";

            var burninationMessageId = await _chatClient.SendMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, burninationMessage);
            await _chatClient.PinMessage(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, burninationMessageId);
        }
    }
}
