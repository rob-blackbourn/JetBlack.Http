using System;
using System.Threading.Tasks;

namespace JetBlack.HttpServer.Routing
{
    public interface IHttpRouter
    {
        Func<HttpRequest, HttpResponse, Task> FindHandler(string path);

        void AddRoute(
            string path,
            Func<HttpRequest, HttpResponse, Task> handler);
    }
}
