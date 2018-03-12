using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ConsoleApp2
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureHostConfiguration(config =>
                {
                    config.AddEnvironmentVariables(); // key "environment"
                })
                .ConfigureAppConfiguration((hostContext, config) =>
                {
                    config.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                        .AddJsonFile("appsettings.json", optional: true)
                        .AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services
                        .AddScoped<IHostedService, App>()
                        .AddSingleton(context.Configuration)
                        .AddOptions()
                        .Configure<Foo>(context.Configuration.GetSection("Foo"));

                })
                .ConfigureLogging((context, config) =>
                {
                    var configuration = context.Configuration.GetSection("Logging");
                    config
                        .AddConfiguration(configuration)
                        .AddConsole()
                        .AddDebug();
                });
            await hostBuilder.RunConsoleAsync();
        }
    }
    class Foo
    {
        public string Bar { get; set; }
    }

    class App : IHostedService
    {
        private readonly ILogger<App> _logger;
        private readonly IConfiguration _configuration;
        private readonly Foo _foo;

        public App(ILogger<App> logger, IOptions<Foo> foo, IConfiguration configuration, IApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _configuration = configuration;
            _foo = foo.Value;
            applicationLifetime.ApplicationStarted.Register(() => { _logger.LogDebug(nameof(applicationLifetime.ApplicationStarted)); });
            applicationLifetime.ApplicationStopping.Register(() => { _logger.LogDebug(nameof(applicationLifetime.ApplicationStopping)); });
            applicationLifetime.ApplicationStopped.Register(() => { _logger.LogDebug(nameof(applicationLifetime.ApplicationStopped)); });
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine(nameof(StartAsync));
            _logger.LogDebug(_configuration["Logging:IncludeScopes"]);
            _logger.LogDebug(nameof(StartAsync));
            _logger.LogDebug(_foo.Bar);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine(nameof(StopAsync));
            _logger.LogDebug(nameof(StopAsync));
            return Task.CompletedTask;
        }
    }
}
