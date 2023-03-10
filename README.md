# jetblack-http

This repo contains an HTTP server for dotnet targeting netStandard2.0/2.1,
which means it supports .Net Framework, as well as Core. It is based on
[HttpListener](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener)
and attempts to maintain the majority of features and supporting classes.

It is a bare-bones implementation, suitable for embedding in applications.

A useful feature of the implementation is that routes may contain variables.
for example the route "http://example.com/api/v1/hello/{name:string}/{age:int}"
can be routed to an appropriate handler, with the path variables resolved
by name and type.

Other features include middleware, fluent configuration, and a replaceable
router. Middleware implementations include compression and CORS.

## Installation

The package can be installed through [nuget](https://www.nuget.org/packages/JetBlack.Http).

## Usage

This is taken from the examples folder:

```csharp
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using JetBlack.Http.Middleware;
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
            })
            {
                var server = new RestServer()
                    .AddPrefix("http://*:8081/")
                    .ConfigureRouter(router => router.IgnoreCase = true)
                    .AddMiddleware(RestCompressionMiddleware.Create())
                    .AddRoute(SayHello, "/api/v1/helloWorld", "GET")
                    .AddRoute(SayWithQueryString, "/api/v1/hello") // GET is the default
                    .AddRoute(SayName, "/api/v1/hello/{name:string}", "GET", "POST")
                    .AddRoute(SayNameAndAge, "/api/v1/hello/{name:string}/{age:int}");

                await server.RunAsync();
            }
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

## Routing

The default REST router allows variables in path names: i.e. `/foo/bar/{name:string}/{age:int}`.
The available types are: `string`, `int`, `double`, `datetime`, `path`.
The `path` type captures the remaining path as a string.

## Middleware

The middleware is nested, is called in order, and has access to both the request
and the response.

Given the following middleware:

```csharp
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
```

The output would be:

```
>FirstMiddleware
>SecondMiddleware
<SecondMiddleware
<FirstMiddleware
```

## Acknowledgements

This was derived from [Cherry](https://github.com/LegendaryB/Cherry).
