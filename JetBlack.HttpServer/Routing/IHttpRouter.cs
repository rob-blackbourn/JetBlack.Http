using System;
using System.Net;
using System.Threading.Tasks;

namespace JetBlack.HttpServer.Routing
{
    public interface IHttpRouter
    {
        Task RouteAsync(HttpRequest req, HttpResponse res, string path);

        void RegisterController(
            string path,
            Func<HttpRequest, HttpResponse, Task> handler);
    }
}
