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
            using (var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            }))
            {
                var server = new RestServer(loggerFactory)
                    .AddPrefix("http://*:8081/")
                    .AddMiddleware(FirstMiddleware)
                    .AddMiddleware(SecondMiddleware)
                    .ConfigureRouter(router => router.IgnoreCase = true)
                    .AddRoute(SayHello, "/sayHello", "GET");

                await server.RunAsync();
            }
        }

        public static Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
        {
            var response = HttpResponse.FromString("Hello, World!");

            return Task.FromResult(response);
        }

        public static async Task<HttpResponse> FirstMiddleware(
            RestRequest request,
            Func<RestRequest, CancellationToken, Task<HttpResponse>> handler,
            CancellationToken token)
        {
            Console.WriteLine(">FirstMiddleware");
            var response = await handler(request, token);
            Console.WriteLine("<FirstMiddleware");
            return response;
        }

        public static async Task<HttpResponse> SecondMiddleware(
            RestRequest request,
            Func<RestRequest, CancellationToken, Task<HttpResponse>> handler,
            CancellationToken token)
        {
            Console.WriteLine(">SecondMiddleware");
            var response = await handler(request, token);
            Console.WriteLine("<SecondMiddleware");
            return response;
        }
    }
}