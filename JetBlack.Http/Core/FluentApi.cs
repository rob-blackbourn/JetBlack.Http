using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Http.Core
{
    public static class FluentApi
    {
        /// <summary>
        /// Add a listener prefix.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="uriPrefix">The listener prefix.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
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

        /// <summary>
        /// Add a route.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="handler">The handler for the route.</param>
        /// <param name="path">The path of the route.</param>
        /// <param name="methods">The allowed HTTP methods.</param>
        /// <typeparam name="TRouter">The class of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The class for the route info.</typeparam>
        /// <typeparam name="TServerInfo">The class of the server info.</typeparam>
        /// <returns>The HTTP server.</returns>
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> AddRoute<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>> handler,
            string path,
            params string[] methods)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            server.Router.AddRoute(handler, path, methods);
            return server;
        }

        /// <summary>
        /// Add middleware.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="handler">The middleware handler.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> AddMiddleware<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Func<HttpRequest<TRouteInfo, TServerInfo>, Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>>, CancellationToken, Task<HttpResponse>> handler)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            server.Middlewares.Add(handler);
            return server;
        }

        /// <summary>
        /// Add a startup handler.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="handler">The startup handler.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> AddStartupHandler<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Func<TServerInfo, CancellationToken, Task> handler)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            server.StartupHandlers.Add(handler);
            return server;
        }

        /// <summary>
        /// Add a shutdown handler.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="handler">The shutdown handler.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> AddShutdownHandler<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Func<TServerInfo, Task> handler)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            server.ShutdownHandlers.Add(handler);
            return server;
        }

        /// <summary>
        /// Configure the server info.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="configure">A function with which the server info is configured.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> ConfigureServerInfo<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Action<TServerInfo> configure)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            configure(server.ServerInfo);
            return server;
        }

        /// <summary>
        /// Configure the listener.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="configure">A function with which the listener is configured.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
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

        /// <summary>
        /// Configure the router.
        /// </summary>
        /// <param name="server">The HTTP server</param>
        /// <param name="configure">A function with which the router is configured.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
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

        /// <summary>
        /// Configure the middleware.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="configure">A function with which the middleware is configured.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> ConfigureMiddleware<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Action<
                IList<
                    Func<
                        HttpRequest<TRouteInfo, TServerInfo>,
                        Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>>,
                        CancellationToken,
                        Task<HttpResponse>>>> configure)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            configure(server.Middlewares);
            return server;
        }

        /// <summary>
        /// Configure the startup handlers.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="configure">A function with which the startup handlers are configured.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> ConfigureStartupHandlers<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Action<IList<Func<TServerInfo, CancellationToken, Task>>> configure)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            configure(server.StartupHandlers);
            return server;
        }

        /// <summary>
        /// Configure the shutdown handlers.
        /// </summary>
        /// <param name="server">The HTTP server.</param>
        /// <param name="configure">A function with which the startup handlers are configured.</param>
        /// <typeparam name="TRouter">The type of the router.</typeparam>
        /// <typeparam name="TRouteInfo">The type of the route information.</typeparam>
        /// <typeparam name="TServerInfo">The type of the server information.</typeparam>
        /// <returns>The HTTP server.</returns>
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> ConfigureShutdownHandlers<TRouter, TRouteInfo, TServerInfo>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            Action<IList<Func<TServerInfo, Task>>> configure)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
        {
            configure(server.ShutdownHandlers);
            return server;
        }
    }
}