using System;
using System.Threading.Tasks;

namespace HttpServer
{
    internal class FuncAsMiddleware : IMiddleware
    {
        private readonly Func<HttpRequest, HttpResponse, Task> _handler;

        public FuncAsMiddleware(Func<HttpRequest, HttpResponse, Task> handler)
        {
            _handler = handler;
        }

        public Task HandleRequestAsync(
            HttpRequest req, 
            HttpResponse res)
        {
            return _handler.Invoke(
                req,
                res);
        }
    }
}
