using System;
using System.Collections.Generic;
using System.Net;
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


        public static HttpServer ConfigureListener(this HttpServer server, Action<HttpListener> configure)
        {
            configure(server.Listener);
            return server;
        }

        public static HttpServer ConfigureRouter(this HttpServer server, Action<IHttpRouter> configure)
        {
            configure(server.Router);
            return server;
        }

        public static HttpServer ConfigureMiddleware(this HttpServer server, Action<IList<Func<HttpRequest, Task>>> configure)
        {
            configure(server.Middlewares);
            return server;
        }
    }
}