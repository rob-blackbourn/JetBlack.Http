using System.Net;
using System.Threading.Tasks;

using JetBlack.HttpServer;

namespace Example
{
    public class HelloWorldController
    {
        public Task<HttpResponse> SayHello(HttpRequest req)
        {
            return Task.FromResult(HttpResponse.FromString("Hello, World!", statusCode: HttpStatusCode.OK));
        }
    }
}