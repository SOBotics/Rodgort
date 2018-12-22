namespace StackExchangeApi
{
    public class StackExchangeApiCredentials : IStackExchangeApiCredentials
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AppKey { get; set; }
        public string AccessToken { get; set; }
    }
}
