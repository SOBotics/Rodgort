using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchangeChat.Authenticators;
using StackExchangeChat.Console.AppSettings;
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

            var credentials = new Credentials();
            config.Bind("ChatCredentials", credentials);
            
            serviceCollection.AddScoped<SiteAuthenticator>();
            serviceCollection.AddScoped<ChatClient>();
            serviceCollection.AddTransient<HttpClient>();
            serviceCollection.AddTransient(_ => new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }));
            serviceCollection.AddTransient<HttpClientWithHandler>();
            serviceCollection.AddSingleton(_ => config);
            serviceCollection.AddSingleton(_ => credentials);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var apiThing = new StackExchangeApiHelper(serviceProvider);

            apiThing.TotalQuestionsByTag("design").GetAwaiter().GetResult();
            apiThing.TotalQuestionsByTag("design").GetAwaiter().GetResult();
            apiThing.TotalQuestionsByTag("design").GetAwaiter().GetResult();

            StackExchangeApiHelper.QuotaRemaining.Subscribe(System.Console.WriteLine);

            //// var result = apiThing.TotalQuestionsByTag("design").GetAwaiter().GetResult();

            //var chatClient = serviceProvider.GetService<ChatClient>();
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
