namespace StackExchangeApi
{
    public interface IStackExchangeApiCredentials
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string AppKey { get; set; }
        string AccessToken { get; set; }
    }
}
