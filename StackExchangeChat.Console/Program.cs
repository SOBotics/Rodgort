using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchangeChat.Console.AppSettings;

namespace StackExchangeChat.Console
{
    class Program
    {
        static void Main(string[] args)
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

            serviceCollection.AddTransient<HttpClient>();
            serviceCollection.AddSingleton(_ => config);
            serviceCollection.AddSingleton(_ => credentials);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var chatClient = new ChatClient(serviceProvider);
        }
    }
}
