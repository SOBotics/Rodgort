namespace StackExchangeChat
{
    public struct ChatSite
    {
        public string ChatDomain { get; }
        public string LoginDomain { get; }

        private ChatSite(string chatDomain, string loginDomain)
        {
            ChatDomain = chatDomain;
            LoginDomain = loginDomain;
        }
        
        public static ChatSite StackOverflow = new ChatSite("chat.stackoverflow.com", "stackoverflow.com");
        public static ChatSite StackExchange = new ChatSite("chat.stackexchange.com", "meta.stackexchange.com");
        public static ChatSite MetaStackExchange = new ChatSite("chat.meta.stackexchange.com", "meta.stackexchange.com");
    }
}
