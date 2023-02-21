using System;
using System.Threading.Tasks;

namespace JetBlack.HttpServer
{
    internal class FuncAsHttpController : HttpController
    {
        private readonly Func<HttpRequest, HttpResponse, Task> _handler;

        internal FuncAsHttpController(Func<HttpRequest, HttpResponse, Task> handler)
        {
            _handler = handler;
        }

        internal override Task HandleAnyAsync(
            HttpRequest req,
            HttpResponse res)
        {
            return _handler.Invoke(
                req,
                res);
        }
    }
}
