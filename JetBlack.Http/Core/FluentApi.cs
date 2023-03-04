using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace JetBlack.Http.Core
{
    public static class FluentApi
    {
        public static HttpServer<TRouter> AddRoute<TRouter>(
            this HttpServer<TRouter> server,
            string path,
            Func<HttpRequest, Task<HttpResponse>> handler)
            where TRouter : class, IHttpRouter
        {
            server.Router.AddRoute(path, handler);
            return server;
        }

        public static HttpServer<TRouter> AddMiddleware<TRouter>(
            this HttpServer<TRouter> server,
            Func<HttpRequest, Task> handler)
            where TRouter : class, IHttpRouter
        {
            server.Middlewares.Add(handler);
            return server;
        }

        public static HttpServer<TRouter> AddPrefix<TRouter>(
            this HttpServer<TRouter> server,
            string uriPrefix)
            where TRouter : class, IHttpRouter
        {
            server.Listener.Prefixes.Add(uriPrefix);
            return server;
        }

        public static HttpServer<TRouter> ConfigureListener<TRouter>(
            this HttpServer<TRouter> server,
            Action<HttpListener> configure)
            where TRouter : class, IHttpRouter
        {
            configure(server.Listener);
            return server;
        }

        public static HttpServer<TRouter> ConfigureRouter<TRouter>(
            this HttpServer<TRouter> server,
            Action<TRouter> configure)
            where TRouter : class, IHttpRouter
        {
            configure(server.Router);
            return server;
        }

        public static HttpServer<TRouter> ConfigureMiddleware<TRouter>(
            this HttpServer<TRouter> server,
            Action<IList<Func<HttpRequest, Task>>> configure)
            where TRouter : class, IHttpRouter
        {
            configure(server.Middlewares);
            return server;
        }
    }
}