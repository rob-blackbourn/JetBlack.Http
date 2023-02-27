# jetblack-http

This repo contains an HTTP server for dotnet targeting netStandard2.0/2.1.

It is a bare-bones implementation, suitable for embedding in applications.

A useful feature of the implementation is that routes may contain variables.
for example the route "http://example.com/api/v1/hello/{name:string}/{age:int}"
can be routed to an appropriate handler, with the path variables resolved
by name and type.

## Installation

The package can be installed through [nuget](https://www.nuget.org/packages/JetBlack.Http/1.0.0-alpha.3).

## Usage

This is taken from the examples folder:

```csharp
using System.Net;
using System.Threading.Tasks;
using JetBlack.Http;

namespace Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new HttpServer()
                .ConfigureListener(listener => listener.Prefixes.Add("http://localhost:8081/"))
                .ConfigureRouter(router => {
                    router.AddRoute("/api/v1/helloWorld", SayHello);
                    router.AddRoute("/api/v1/hello/{name:string}", SayName);
                    router.AddRoute("/api/v1/hello/{name:string}/{age:int}", SayNameAndAge);
                });


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
    }
}
```

## Acknowledgements

This was derived from [Cherry](https://github.com/LegendaryB/Cherry).
