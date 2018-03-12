using System;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                .AddJsonFile("appsettings.json", optional: false)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var configuration = builder.Build();

            var loggerFactory = new LoggerFactory()
                .AddConsole(configuration.GetSection("Logging:Console"))
                .AddDebug();

            IServiceCollection services = new ServiceCollection();

            services
                .AddSingleton(loggerFactory)
                .AddLogging();

            services
                .AddSingleton(configuration)
                .AddOptions()
                .Configure<Foo>(configuration.GetSection("Foo"));

            services.AddTransient<App>();

            var serviceProvider = services.BuildServiceProvider();

            var app = (App)serviceProvider.GetService(typeof(App));
            app.Run();

            Console.ReadKey();
        }
    }

    class Foo
    {
        public string Bar { get; set; }
    }

    class App
    {
        private readonly ILogger<App> _logger;
        private readonly Foo _foo;

        public App(ILogger<App> logger, IOptions<Foo> foo)
        {
            _logger = logger;
            _foo = foo.Value;
        }
        public void Run()
        {
            _logger.LogDebug(_foo.Bar);
        }
    }
}
