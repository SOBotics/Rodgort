using System;

namespace StackExchangeChat
{
    public class ChatClient
    {
        private readonly IServiceProvider _serviceProvider;

        public ChatClient(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
    }
}
