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
                var controller = new GreetingController(loggerFactory);

                var server = new RestServer(loggerFactory)
                    .AddPrefix("http://*:8081/")
                    .ConfigureRouter(router => router.IgnoreCase = true)
                    .AddRoute(controller.SayHello, "/api/v1/helloWorld", "GET")
                    .AddRoute(controller.SayWithQueryString, "/api/v1/hello")
                    .AddRoute(controller.SayName, "/api/v1/hello/{name:string}", "GET", "POST")
                    .AddRoute(controller.SayNameAndAge, "/api/v1/hello/{name:string}/{age:int}");

                await server.RunAsync();
            }
        }
    }

    internal class GreetingController
    {
        private readonly ILogger _logger;

        public GreetingController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GreetingController>();
        }

        public Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
        {
            _logger.LogTrace("In SayHello");

            var response = HttpResponse.FromString(
                "Hello, World!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public Task<HttpResponse> SayName(RestRequest request, CancellationToken token)
        {
            _logger.LogTrace("In SayName");

            var name = request.RouteInfo.Matches["name"];

            var response = HttpResponse.FromString(
                $"Hello, {name}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public Task<HttpResponse> SayNameAndAge(RestRequest request, CancellationToken token)
        {
            _logger.LogTrace("In SayNameAndAge");

            var name = request.RouteInfo.Matches["name"];
            var age = request.RouteInfo.Matches["age"];

            var response = HttpResponse.FromString(
                $"Hello, {name}, you are {age}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public Task<HttpResponse> SayWithQueryString(RestRequest request, CancellationToken token)
        {
            _logger.LogTrace("In SayWithQueryString");

            var name = request.Request.QueryString.Get("name");
            var age = request.Request.QueryString.Get("age");

            var response = HttpResponse.FromString(
                $"Hello, {name ?? "nobody"}, you are {age ?? "a mystery"}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }
    }
}