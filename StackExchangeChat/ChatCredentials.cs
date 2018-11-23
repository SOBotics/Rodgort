namespace StackExchangeChat
{
    public class ChatCredentials : IChatCredentials
    {
        public string AcctCookie { get; set; }
        public string AcctCookieExpiry { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
    }
}
