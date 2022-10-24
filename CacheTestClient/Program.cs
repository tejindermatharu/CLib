// See https://aka.ms/new-console-template for more information
using CacheLib;
using CacheTestClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

internal class Program
{
    private static async Task Main(string[] args)
    {
        // Setup Host
        //var configuration = new ConfigurationBuilder()
        //     .SetBasePath(Directory.GetCurrentDirectory())
        //     .AddJsonFile($"appsettings.json")
        //     .AddEnvironmentVariables()
        //     .AddCommandLine(args)
        //     .Build();

        //var serviceProvider = new ServiceCollection()
        //    .AddSingleton<IConfiguration>(configuration)
        //    .BuildServiceProvider();

        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) =>
            {
                services
                    .AddHostedService<ConsoleHostedService>();
            })
            .Build();

        // Kicks off the ConsoleHostedService.StartAsync method
        await host.RunAsync();

        CustomCache.Initialise(3);
    }
}