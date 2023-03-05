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
            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            var server = new RestServer(loggerFactory)
                .ConfigureListener(listener => listener.Prefixes.Add("http://*:8081/"))
                .ConfigureRouter(router =>
                {
                    router.IgnoreCase = true;

                    router.AddRoute(QuickHandler, "/api/v1/quick");
                    router.AddRoute(SlowHandler, "/api/v1/slow");
                });

            await server.RunAsync();
        }


        public static Task<HttpResponse> QuickHandler(RestRequest request)
        {
            var response = HttpResponse.FromString($"Quick: {DateTime.Now}");

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SlowHandler(RestRequest request)
        {
            Thread.Sleep(TimeSpan.FromSeconds(30));

            var response = HttpResponse.FromString($"Slow: {DateTime.Now}");

            return Task.FromResult(response);
        }
    }
}