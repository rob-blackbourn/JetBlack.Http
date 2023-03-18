using System;
using System.Net;
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
                    .AddPrefix("http://*:8081/")
                    .AddStartupHandler(FirstStartupHandler)
                    .AddStartupHandler(SecondStartupHandler)
                    .AddShutdownHandler(FirstShutdownHandler)
                    .AddShutdownHandler(SecondShutdownHandler)
                    .AddRoute(SayHello, "/hello");

                await server.RunAsync(token);
            }
        }

        public static Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
        {
            var response = HttpResponse.FromString("Hello, World!");

            return Task.FromResult(response);
        }

        public static Task FirstStartupHandler(RestServerInfo serverInfo, CancellationToken token)
        {
            Console.WriteLine("FirstStartupHandler");

            return Task.CompletedTask;
        }

        public static Task SecondStartupHandler(RestServerInfo serverInfo, CancellationToken token)
        {
            Console.WriteLine("SecondStartupHandler");

            return Task.CompletedTask;
        }

        public static Task FirstShutdownHandler(RestServerInfo serverInfo)
        {
            Console.WriteLine("FirstShutdownHandler");

            return Task.CompletedTask;
        }

        public static Task SecondShutdownHandler(RestServerInfo serverInfo)
        {
            Console.WriteLine("SecondShutdownHandler");

            return Task.CompletedTask;
        }
    }
}