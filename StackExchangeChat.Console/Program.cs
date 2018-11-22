using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchangeChat.Console.AppSettings;
using StackExchangeChat.Sites;

namespace StackExchangeChat.Console
{
    class Program
    {
        public static void Main(string[] args)
        {
            System.Console.WriteLine("Hello World!");

            var serviceCollection = new ServiceCollection();
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true)
                .AddJsonFile("appsettings.dev.json", true);

            IConfiguration config = builder.Build();

            var credentials = new Credentials();
            config.Bind("ChatCredentials", credentials);

            serviceCollection.AddTransient(_ =>
            {
                var handler = new HttpClientHandler();
                var httpClient = new HttpClient(handler);
                return new HttpClientWithHandler
                {
                    HttpClient = httpClient,
                    HttpClientHandler = handler
                };
            });

            serviceCollection.AddSingleton(_ => config);
            serviceCollection.AddSingleton(_ => credentials);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var chatClient = new ChatClient(serviceProvider);
            chatClient.SubscribeToEvents(Site.StackOverflow, 167908)
                .Where(c => c.EventType == EventType.MessagePosted || c.EventType == EventType.MessageEdited)
                .Subscribe(async chatEvent =>
                {
                    await chatClient.SendMessage(Site.StackOverflow, 167908, $":{chatEvent.MessageId} Replying to message..");
                }, exception =>
                {
                    System.Console.WriteLine(exception);
                });

            System.Console.ReadKey();
        }
    }
}
