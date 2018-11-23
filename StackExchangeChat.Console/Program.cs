using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchangeApi;
using StackExchangeChat.Utilities;

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

            var credentials = new ChatCredentials();
            config.Bind("ChatCredentials", credentials);
            
            serviceCollection.AddScoped<SiteAuthenticator>();
            serviceCollection.AddScoped<ChatClient>();
            serviceCollection.AddTransient<HttpClient>();
            serviceCollection.AddTransient(_ => new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }));
            serviceCollection.AddTransient<HttpClientWithHandler>();
            serviceCollection.AddSingleton(_ => config);
            serviceCollection.AddSingleton<IChatCredentials>(_ => credentials);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var apiThing = new ApiClient(serviceProvider);

            //apiThing.TotalQuestionsByTag("stackoverflow", "design").GetAwaiter().GetResult();
            //apiThing.TotalQuestionsByTag("stackoverflow", "design").GetAwaiter().GetResult();
            //apiThing.TotalQuestionsByTag("stackoverflow", "design").GetAwaiter().GetResult();

            var result = apiThing.QuestionsByTag("meta.stackoverflow", "burninate-request").GetAwaiter().GetResult();

            ApiClient.QuotaRemaining.Subscribe(System.Console.WriteLine);

            //// var result = apiThing.TotalQuestionsByTag("design").GetAwaiter().GetResult();

            var chatClient = serviceProvider.GetService<ChatClient>();
            //chatClient.SubscribeToEvents(Site.StackOverflow, 167908)
            //    .Where(c => c.EventType == EventType.MessagePosted || c.EventType == EventType.MessageEdited)
            //    .Subscribe(async chatEvent =>
            //    {
            //        await chatClient.SendMessage(Site.StackOverflow, 167908, $":{chatEvent.MessageId} Replying to message..");
            //    }, exception =>
            //    {
            //        System.Console.WriteLine(exception);
            //    });

            System.Console.ReadKey();
        }
    }
}
