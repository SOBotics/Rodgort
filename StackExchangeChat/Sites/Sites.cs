namespace StackExchangeChat.Sites
{
    public class Site
    {
        public string ChatDomain { get; }
        public string LoginDomain { get; }

        private Site(string chatDomain, string loginDomain)
        {
            ChatDomain = chatDomain;
            LoginDomain = loginDomain;
        }
        
        public static Site StackOverflow = new Site("chat.stackoverflow.com", "stackoverflow.com");
        public static Site StackExchange = new Site("chat.stackexchange.com", "gaming.stackexchange.com");
        public static Site MetaStackExchange = new Site("chat.meta.stackexchange.com", "meta.stackexchange.com");
    }
}
