using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace JetBlack.Http.Core
{
    public static class FluentApi
    {
        public static HttpServer<TRouter, TRouteInfo> AddRoute<TRouter, TRouteInfo>(
            this HttpServer<TRouter, TRouteInfo> server,
            string path,
            Func<HttpRequest<TRouteInfo>, Task<HttpResponse>> handler)
            where TRouter : class, IHttpRouter<TRouteInfo>
            where TRouteInfo : class
        {
            server.Router.AddRoute(path, handler);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo> AddMiddleware<TRouter, TRouteInfo>(
            this HttpServer<TRouter, TRouteInfo> server,
            Func<HttpRequest<TRouteInfo>, Task> handler)
            where TRouter : class, IHttpRouter<TRouteInfo>
            where TRouteInfo : class
        {
            server.Middlewares.Add(handler);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo> AddPrefix<TRouter, TRouteInfo>(
            this HttpServer<TRouter, TRouteInfo> server,
            string uriPrefix)
            where TRouter : class, IHttpRouter<TRouteInfo>
            where TRouteInfo : class
        {
            server.Listener.Prefixes.Add(uriPrefix);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo> ConfigureListener<TRouter, TRouteInfo>(
            this HttpServer<TRouter, TRouteInfo> server,
            Action<HttpListener> configure)
            where TRouter : class, IHttpRouter<TRouteInfo>
            where TRouteInfo : class
        {
            configure(server.Listener);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo> ConfigureRouter<TRouter, TRouteInfo>(
            this HttpServer<TRouter, TRouteInfo> server,
            Action<TRouter> configure)
            where TRouter : class, IHttpRouter<TRouteInfo>
            where TRouteInfo : class
        {
            configure(server.Router);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo> ConfigureMiddleware<TRouter, TRouteInfo>(
            this HttpServer<TRouter, TRouteInfo> server,
            Action<IList<Func<HttpRequest<TRouteInfo>, Task>>> configure)
            where TRouter : class, IHttpRouter<TRouteInfo>
            where TRouteInfo : class
        {
            configure(server.Middlewares);
            return server;
        }
    }
}