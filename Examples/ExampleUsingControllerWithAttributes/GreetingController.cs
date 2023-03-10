using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using JetBlack.Http.Rest;

namespace Example
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;

    internal class GreetingController
    {
        private readonly ILogger _logger;

        public GreetingController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GreetingController>();
        }

        public void MethodWithoutAttribute()
        {
            // Only methods with attributes get included.
        }

        [Route("/api/v1/helloWorld", "GET")]
        public Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
        {
            _logger.LogTrace("In SayHello");

            var response = HttpResponse.FromString(
                "Hello, World!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        [Route("/api/v1/hello/{name:string}", "GET", "POST")]
        public Task<HttpResponse> SayName(RestRequest request, CancellationToken token)
        {
            _logger.LogTrace("In SayName");

            var name = request.RouteInfo.Matches["name"];

            var response = HttpResponse.FromString(
                $"Hello, {name}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }

        [Route("/api/v1/hello/{name:string}/{age:int}")]
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

        [Route("/api/v1/hello")]
        public Task<HttpResponse> SayWithQueryString(RestRequest request, CancellationToken token)
        {
            _logger.LogTrace("In SayWithQueryString");

            var name = request.Context.Request.QueryString.Get("name");
            var age = request.Context.Request.QueryString.Get("age");

            var response = HttpResponse.FromString(
                $"Hello, {name ?? "nobody"}, you are {age ?? "a mystery"}!",
                statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }
    }
}