﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using JetBlack.Http.Rest;

namespace Example
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;

    internal class Program
    {
        static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var serverTask = Task.Run(
                () => RunTheServer(cancellationTokenSource.Token));

            Console.WriteLine("Press ENTER to stop the HTTP server.");
            Console.ReadLine();

            Console.WriteLine("Cancelling");
            cancellationTokenSource.Cancel();
            Console.WriteLine("Waiting for the server to stop");
            try
            {
                await serverTask;
            }
            catch (Exception error)
            {
                Console.WriteLine($"Error {error.Message}");
            }

            Console.WriteLine("The server has shut down");
        }

        static async Task RunTheServer(CancellationToken token)
        {
            using (var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            }))
            {
                var server = new RestServer(loggerFactory)
                    .ConfigureListener(listener => listener.Prefixes.Add("http://localhost:8081/"))
                    .ConfigureRouter(router =>
                    {
                        router.IgnoreCase = true;

                        router.AddRoute(QuickHandler, "/quick");
                        router.AddRoute(SlowHandler, "/slow");
                    });

                await server.RunAsync(token);
            }
        }


        public static Task<HttpResponse> QuickHandler(RestRequest request, CancellationToken token)
        {
            var response = HttpResponse.FromString($"Quick: {DateTime.Now}");

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SlowHandler(RestRequest request, CancellationToken token)
        {
            token.WaitHandle.WaitOne(TimeSpan.FromSeconds(30));

            var response = HttpResponse.FromString($"Slow: {DateTime.Now}");

            return Task.FromResult(response);
        }
    }
}