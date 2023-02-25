using System.Net;
using System.Threading.Tasks;
using JetBlack.Http;

namespace Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var server = new HttpServer(() =>
            {
                var listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:8081/");

                return listener;
            })
            .AddRoute("/api/v1/helloWorld", SayHello);

            await server.RunAsync();
        }

        public static Task<HttpResponse> SayHello(HttpRequest req)
        {
            return Task.FromResult(HttpResponse.FromString("Hello, World!", statusCode: HttpStatusCode.OK));
        }

    }
}