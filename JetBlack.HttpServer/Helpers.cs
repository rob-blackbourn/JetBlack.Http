using System;
using System.Threading.Tasks;

namespace JetBlack.Http
{
    public static class Helpers
    {
        public static HttpServer AddRoute(this HttpServer server, string path, Func<HttpRequest, Task<HttpResponse>> handler)
        {
            server.Router.AddRoute(path, handler);
            return server;
        }

        public static HttpServer AddMiddleware(this HttpServer server, Func<HttpRequest, Task> handler)
        {
            server.Middlewares.Add(handler);
            return server;
        }
    }
}