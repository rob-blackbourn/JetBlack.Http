using System;
using System.Collections.Generic;
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
                // Setup the listener.
                var listener = new HttpListener();
                listener.Prefixes.Add("http://*:8081/");

                // Setup the router.
                var router = new RestRouter(true, loggerFactory);
                router.AddRoute(SayHello, "/api/v1/helloWorld", "GET");
                router.AddRoute(SayWithQueryString, "/api/v1/hello"); // GET is the default.
                router.AddRoute(SayName, "/api/v1/hello/{name:string}", "GET", "POST");
                router.AddRoute(SayNameAndAge, "/api/v1/hello/{name:string}/{age:int}");

                // Make a list of middlewares.
                var middlewares = new List<Func<HttpRequest<RestRouteInfo, RestServerInfo>, Task>>();

                // Make the server.
                var server = new RestServer(listener, router, middlewares, loggerFactory);

                // Start the server.
                await server.RunAsync();
            }
        }

        public static Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
        {
            var response = HttpResponse.FromString(
                "Hello, World!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayName(RestRequest request, CancellationToken token)
        {
            var name = request.RouteInfo.Matches["name"];

            var response = HttpResponse.FromString(
                $"Hello, {name}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayNameAndAge(RestRequest request, CancellationToken token)
        {
            var name = request.RouteInfo.Matches["name"];
            var age = request.RouteInfo.Matches["age"];

            var response = HttpResponse.FromString(
                $"Hello, {name}, you are {age}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayWithQueryString(RestRequest request, CancellationToken token)
        {
            var name = request.Request.QueryString.Get("name");
            var age = request.Request.QueryString.Get("age");

            var response = HttpResponse.FromString(
                $"Hello, {name ?? "nobody"}, you are {age ?? "a mystery"}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }
    }
}