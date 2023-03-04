using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using JetBlack.Http.Rest;


namespace Example
{
    using RestRequest = HttpRequest<RestRouteInfo>;

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
                .AddRoute("/api/v1/helloWorld", SayHello)
                .AddRoute("/api/v1/hello", SayWithQueryString)
                .AddRoute("/api/v1/hello/{name:string}", SayName)
                .AddRoute("/api/v1/hello/{name:string}/{age:int}", SayNameAndAge);

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
            if (request?.RouteInfo?.Matches == null)
                return Task.FromResult(new HttpResponse(HttpStatusCode.BadRequest));

            var response = HttpResponse.FromString(
                $"Hello, {request.RouteInfo.Matches["name"]}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        public static Task<HttpResponse> SayNameAndAge(RestRequest request)
        {
            if (request?.RouteInfo?.Matches == null)
                return Task.FromResult(new HttpResponse(HttpStatusCode.BadRequest));

            var response = HttpResponse.FromString(
                $"Hello, {request.RouteInfo.Matches["name"]}, you are {request.RouteInfo.Matches["age"]}!",
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