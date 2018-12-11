namespace StackExchangeApi
{
    public class PagingOptions
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 30;

        public bool AutoFetchAll { get; set; }
    }
}
