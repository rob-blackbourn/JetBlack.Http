﻿using System;
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
                router.AddRoute(SayHello, "/helloWorld", "GET");
                router.AddRoute(SayWithQueryString, "/hello"); // GET is the default.
                router.AddRoute(SayName, "/hello/{name:string}", "GET", "POST");
                router.AddRoute(SayNameAndAge, "/hello/{name:string}/{age:int}");

                // Make a list of middlewares.
                var middlewares = new List<
                    Func<
                        HttpRequest<RestRouteInfo, RestServerInfo>,
                        Func<HttpRequest<RestRouteInfo, RestServerInfo>, CancellationToken, Task<HttpResponse>>,
                        CancellationToken,
                        Task<HttpResponse>>>();

                // Make a list of startup and shutdown handlers.
                var startupHandlers = new List<Func<RestServerInfo, CancellationToken, Task>>();
                var shutdownHandlers = new List<Func<RestServerInfo, Task>>();

                // Make the server.
                var server = new RestServer(
                    listener,
                    router,
                    middlewares,
                    startupHandlers,
                    shutdownHandlers,
                    loggerFactory);

                // Start the server.
                await server.RunAsync();
            }
        }

        public static Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
        {
            var response = HttpResponse.FromString("Hello, World!");

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayName(RestRequest request, CancellationToken token)
        {
            var name = request.RouteInfo.Matches["name"];

            var response = HttpResponse.FromString($"Hello, {name}!");

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayNameAndAge(RestRequest request, CancellationToken token)
        {
            var name = request.RouteInfo.Matches["name"];
            var age = request.RouteInfo.Matches["age"];

            var response = HttpResponse.FromString($"Hello, {name}, you are {age}!");

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayWithQueryString(RestRequest request, CancellationToken token)
        {
            var name = request.Context.Request.QueryString.Get("name");
            var age = request.Context.Request.QueryString.Get("age");

            var response = HttpResponse.FromString(
                $"Hello, {name ?? "nobody"}, you are {age ?? "a mystery"}!");

            return Task.FromResult(response);
        }
    }
}