using System;
using System.Collections.Generic;
using System.Net;
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
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            });

            var listener = new HttpListener();
            listener.Prefixes.Add("http://*:8081/");

            var router = new RestRouter(true, loggerFactory);
            router.AddRoute(SayHello, "/api/v1/helloWorld");
            router.AddRoute(SayWithQueryString, "/api/v1/hello");
            router.AddRoute(SayName, "/api/v1/hello/{name:string}");
            router.AddRoute(SayNameAndAge, "/api/v1/hello/{name:string}/{age:int}");

            var middlewares = new List<Func<HttpRequest<RestRouteInfo, RestServerInfo>, Task>>();

            var server = new RestServer(listener, router, middlewares, loggerFactory);
            await server.RunAsync();
        }

        public static Task<HttpResponse> SayHello(RestRequest request)
        {
            var response = HttpResponse.FromString(
                "Hello, World!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayName(RestRequest request)
        {
            var name = request.RouteInfo.Matches["name"];

            var response = HttpResponse.FromString(
                $"Hello, {name}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayNameAndAge(RestRequest request)
        {
            var name = request.RouteInfo.Matches["name"];
            var age = request.RouteInfo.Matches["age"];

            var response = HttpResponse.FromString(
                $"Hello, {name}, you are {age}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayWithQueryString(RestRequest request)
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