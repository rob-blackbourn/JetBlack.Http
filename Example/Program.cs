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