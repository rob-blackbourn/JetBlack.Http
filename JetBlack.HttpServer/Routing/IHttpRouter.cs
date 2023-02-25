using System;
using System.Net;
using System.Threading.Tasks;

namespace JetBlack.HttpServer.Routing
{
    public interface IHttpRouter
    {
        Task RouteAsync(HttpListenerContext ctx);

        void RegisterMiddleware<TMiddleware>(
            string route,
            TMiddleware middleware)

            where TMiddleware : class, IMiddleware;

        void RegisterController(
            string path,
            Func<HttpRequest, HttpResponse, Task> handler);
    }
}
