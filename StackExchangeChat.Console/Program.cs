using System.IO;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rodgort.Services;
using Rodgort.Utilities;
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
            serviceCollection.AddScoped<NewBurninationService>();
            
            serviceCollection.AddTransient(_ => new HttpClientWithHandler(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }));

            serviceCollection.AddSingleton(_ => config);
            serviceCollection.AddSingleton<IChatCredentials>(_ => credentials);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            //apiThing.TotalQuestionsByTag("stackoverflow", "design").GetAwaiter().GetResult();
            //apiThing.TotalQuestionsByTag("stackoverflow", "design").GetAwaiter().GetResult();
            //apiThing.TotalQuestionsByTag("stackoverflow", "design").GetAwaiter().GetResult();

            //var result = apiThing.QuestionsByTag("meta.stackoverflow", "burninate-request").GetAwaiter().GetResult();

            //ApiClient.QuotaRemaining.Subscribe(System.Console.WriteLine);

            // var result = apiThing.TotalQuestionsByTag("design").GetAwaiter().GetResult();

            var chatClient = serviceProvider.GetService<ChatClient>();

            var newBurninationService = serviceProvider.GetService<NewBurninationService>();

            newBurninationService.CreateRoomForBurn(ChatSite.StackOverflow, ChatRooms.SO_BOTICS_WORKSHOP, "priority", "https://meta.stackoverflow.com/questions/285084/should-we-burninate-the-priority-tag").GetAwaiter().GetResult();
            // var roomId = chatClient.CreateRoom(ChatSite.StackOverflow, 167908, "This is a testing room", "This is a testing description").GetAwaiter().GetResult();
            //var events = chatClient.SubscribeToEvents(ChatSite.StackExchange, 86421);
            //events.Subscribe(System.Console.WriteLine);

            //chatClient
            //    .SubscribeToEvents(ChatSite.StackExchange, 86421)
            //    .OnlyMessages()
            //    .SameRoomOnly()
            //    .SkipMyMessages()
            //    .Subscribe(async chatEvent =>
            //    {
            //        try
            //        {
            //            await chatClient.SendMessage(chatEvent.RoomDetails.ChatSite, chatEvent.RoomDetails.RoomId, $":{chatEvent.ChatEventDetails.MessageId} Replying to message..");
            //        }
            //        catch (Exception) { }
            //    }, exception =>
            //    {
            //        System.Console.WriteLine(exception);
            //    });

            System.Console.ReadKey();
        }
    }
}
