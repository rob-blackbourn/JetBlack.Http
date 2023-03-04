﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http;

namespace Example
{
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
            router.AddRoute("/api/v1/helloWorld", SayHello);
            router.AddRoute("/api/v1/hello", SayWithQueryString);
            router.AddRoute("/api/v1/hello/{name:string}", SayName);
            router.AddRoute("/api/v1/hello/{name:string}/{age:int}", SayNameAndAge);

            var middlewares = new List<Func<HttpRequest, Task>>();

            var server = new RestServer(listener, router, middlewares, loggerFactory);
            await server.RunAsync();
        }

        public static Task<HttpResponse> SayHello(HttpRequest request)
        {
            var response = HttpResponse.FromString(
                "Hello, World!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayName(HttpRequest request)
        {
            if (request.Matches == null)
                return Task.FromResult(new HttpResponse(HttpStatusCode.BadRequest));

            var response = HttpResponse.FromString(
                $"Hello, {request.Matches["name"]}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayNameAndAge(HttpRequest request)
        {
            if (request.Matches == null)
                return Task.FromResult(new HttpResponse(HttpStatusCode.BadRequest));

            var response = HttpResponse.FromString(
                $"Hello, {request.Matches["name"]}, you are {request.Matches["age"]}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayWithQueryString(HttpRequest request)
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