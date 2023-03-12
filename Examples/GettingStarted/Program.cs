﻿using System.Net;
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
                    .AddPrefix("http://localhost:8081/")
                    .AddRoute(IndexHandler, "/index.html", "GET");

                await server.RunAsync();
            }
        }

        public static Task<HttpResponse> IndexHandler(
            RestRequest request,
            CancellationToken token)
        {
            var page = @"
<!DOCTYPE html>
<html lang='en'>
  <head>
    <meta charset='utf-8'>
    <title>title</title>
  </head>
  <body>
    <h1>Getting Started</h1>
    <p>This is a good place to start<p>
  </body>
</html>
";
            var response = HttpResponse.FromString(
                page,
                HttpStatusCode.OK,
                "text/html");

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
            var name = request.Context.Request.QueryString.Get("name");
            var age = request.Context.Request.QueryString.Get("age");

            var response = HttpResponse.FromString(
                $"Hello, {name ?? "nobody"}, you are {age ?? "a mystery"}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }
    }
}