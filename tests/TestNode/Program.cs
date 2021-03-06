﻿using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NeoFx.P2P;

namespace NeoFx.TestNode
{
    class Program
    {
        public static Task Main(string[] args)
        {
            return CreateHostBuilder(args).Build().RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            if (!Directory.Exists("logs"))
            {
                Directory.CreateDirectory("logs");
            }

            var dateTimeString = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss");
            var logFilename = $"logs/app-{dateTimeString}.log";

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging((_, builder) => builder.AddFile(logFilename))
                // .UseWindowsService()
                // .UseSystemd()
                .ConfigureServices((context, services) =>
                {
                    services.Configure<NodeOptions>(context.Configuration.GetSection("NodeOptions"))
                        .Configure<NetworkOptions>(context.Configuration.GetSection("NetworkOptions"))
                        .AddTransient<IPipelineSocket, PipelineSocket>()
                        .AddTransient<IRemoteNode, RemoteNode>()
                        .AddTransient<IRemoteNodeFactory, RemoteNodeFactory>()
                        .AddSingleton<IBlockchain, Blockchain>()
                        .AddSingleton<IStorage, Storage>()
                        .AddTransient<IRemoteNodeManager, RemoteNodeManager>()
                        .AddHostedService<LocalNode>();
                });
        }
    }
}
