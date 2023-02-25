using System;
using System.Net;
using System.Threading.Tasks;

namespace JetBlack.HttpServer.Routing
{
    public interface IHttpRouter
    {
        Task RouteAsync(HttpListenerContext ctx);

        void RegisterMiddleware(
            Func<HttpRequest, HttpResponse, Task> middleware);

        void RegisterController(
            string path,
            Func<HttpRequest, HttpResponse, Task> handler);
    }
}
