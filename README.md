# jetblack-http

This repo contains an HTTP server for dotnet targeting netStandard2.0/2.1.

It is a bare-bones implementation, suitable for embedding in applications.

A useful feature of the implementation is that routes may contain variables.
for example the route "http://example.com/api/v1/hello/{name:string}/{age:int}"
can be routed to an appropriate handler, with the path variables resolved
by name and type.

Other features include middleware, fluent configuration, and a replaceable
router.

## Installation

The package can be installed through [nuget](https://www.nuget.org/packages/JetBlack.Http/1.0.0-alpha.3).

## Usage

This is taken from the examples folder:

```csharp
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

            var server = new RestServer()
                .AddPrefix("http://*:8081/")
                .ConfigureRouter(router => router.IgnoreCase = true)
                .AddRoute(SayHello, "/api/v1/helloWorld", "GET")
                .AddRoute(SayWithQueryString, "/api/v1/hello") // GET is the default
                .AddRoute(SayName, "/api/v1/hello/{name:string}", "GET", "POST")
                .AddRoute(SayNameAndAge, "/api/v1/hello/{name:string}/{age:int}");

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
```

## Acknowledgements

This was derived from [Cherry](https://github.com/LegendaryB/Cherry).
