using System.Net;
using System.Threading.Tasks;

using JetBlack.HttpServer;

namespace Example
{
    public class HelloWorldController : HttpController
    {
        public override Task GetAsync(HttpRequest req, HttpResponse res)
        {
            return res.AnswerWithStatusCodeAsync(
                "Hello World!",
                HttpStatusCode.OK);
        }
    }
}