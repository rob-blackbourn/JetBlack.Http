using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace JetBlack.Http.Core
{
    public static class FluentApi
    {
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> AddRoute<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Func<HttpRequest<TRouteInfo, TServerInfo>, Task<HttpResponse>> handler,
            string path,
            params string[] methods)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            server.Router.AddRoute(handler, path, methods);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo, TServerInfo> AddMiddleware<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Func<HttpRequest<TRouteInfo, TServerInfo>, Task> handler)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            server.Middlewares.Add(handler);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo, TServerInfo> AddPrefix<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            string uriPrefix)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            server.Listener.Prefixes.Add(uriPrefix);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo, TServerInfo> ConfigureListener<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Action<HttpListener> configure)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            configure(server.Listener);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo, TServerInfo> ConfigureRouter<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Action<TRouter> configure)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            configure(server.Router);
            return server;
        }

        public static HttpServer<TRouter, TRouteInfo, TServerInfo> ConfigureMiddleware<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Action<IList<Func<HttpRequest<TRouteInfo, TServerInfo>, Task>>> configure)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            configure(server.Middlewares);
            return server;
        }
    }
}