using System;
using System.Threading.Tasks;

namespace JetBlack.HttpServer.Routing
{
    public interface IHttpRouter
    {
        Func<HttpRequest, HttpResponse, Task> RouteAsync(string path);

        void RegisterController(
            string path,
            Func<HttpRequest, HttpResponse, Task> handler);
    }
}
