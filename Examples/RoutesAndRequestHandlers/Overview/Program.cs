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
                    .ConfigureRouter(router => router.IgnoreCase = true)
                    .AddRoute(SayHello, "/helloWorld", "GET")
                    .AddRoute(SayWithQueryString, "/hello")
                    .AddRoute(SayName, "/hello/{name:string}", "GET", "POST")
                    .AddRoute(SayNameAndAge, "/hello/{name:string}/{age:int}");

                await server.RunAsync();
            }
        }

        public static Task<HttpResponse> SayHello(
            RestRequest request,
            CancellationToken token)
        {
            var response = HttpResponse.FromString("Hello, World!");

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayName(
            RestRequest request,
            CancellationToken token)
        {
            var name = request.RouteInfo.Matches["name"];

            var response = HttpResponse.FromString($"Hello, {name}!");

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayNameAndAge(
            RestRequest request,
            CancellationToken token)
        {
            var name = request.RouteInfo.Matches["name"];
            var age = request.RouteInfo.Matches["age"];

            var response = HttpResponse.FromString($"Hello, {name}, you are {age}!");

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayWithQueryString(
            RestRequest request,
            CancellationToken token)
        {
            var name = request.Context.Request.QueryString.Get("name");
            var age = request.Context.Request.QueryString.Get("age");

            var response = HttpResponse.FromString(
                $"Hello, {name ?? "nobody"}, you are {age ?? "a mystery"}!");

            return Task.FromResult(response);
        }
    }
}