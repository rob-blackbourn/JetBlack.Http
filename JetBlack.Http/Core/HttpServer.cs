using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JetBlack.Http.Core
{
    /// <summary>
    /// An Http server.
    /// </summary>
    /// 
    /// <typeparam name="TRouter">The type of the class used for routing.</typeparam>
    /// <typeparam name="TRouteInfo">The type of the class used for route information.</typeparam>
    /// <typeparam name="TServerInfo">The type of the class used for server information.</typeparam>
    public class HttpServer<TRouter, TRouteInfo, TServerInfo>
        where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
        where TRouteInfo : class
        where TServerInfo : class
    {
        private readonly ILogger<HttpServer<TRouter, TRouteInfo, TServerInfo>> _logger;
        private readonly TServerInfo _serverInfo;

        /// <summary>
        /// Create an HTTP Server.
        /// </summary>
        /// 
        /// <param name="routerFactory">A factory to create the router.</param>
        /// <param name="serverInfo">The server info.</param>
        /// <param name="listener">An optional listener.</param>
        /// <param name="middlewares">Optional middlewares</param>
        /// <param name="loggerFactory">An optional logger factory.</param>
        public HttpServer(
            Func<ILoggerFactory, TRouter> routerFactory,
            TServerInfo serverInfo,
            HttpListener? listener = null,
            IList<Func<HttpRequest<TRouteInfo, TServerInfo>, Task>>? middlewares = null,
            ILoggerFactory? loggerFactory = null)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<HttpServer<TRouter, TRouteInfo, TServerInfo>>();

            Listener = listener ?? new HttpListener();
            Middlewares = middlewares ?? new List<Func<HttpRequest<TRouteInfo, TServerInfo>, Task>>();
            Router = routerFactory(loggerFactory);
            _serverInfo = serverInfo;
        }

        internal HttpListener Listener { get; }
        internal IList<Func<HttpRequest<TRouteInfo, TServerInfo>, Task>> Middlewares { get; }
        internal TRouter Router { get; }

        /// <summary>
        /// Run the server.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>A task awaitable.</returns>
        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting.");

            try
            {
                _logger.LogInformation("Starting the listener.");

                Listener.Start();

                _logger.LogInformation(
                    "Listening on [{Bindings}].",
                    string.Join(",", Listener.Prefixes));

                while (!cancellationToken.IsCancellationRequested)
                {
                    var context = await Listener.GetContextAsync();
                    await HandleRequestAsync(context);
                }

                _logger.LogInformation("Stopping the listener.");

                Listener.Stop();
            }
            catch (Exception error)
            {
                _logger.LogCritical(error, "The listener has faulted.");
                throw;
            }

            _logger.LogInformation("Stopped.");
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                // Find a handler for the route.
                var (handler, routeInfo) = Router.FindHandler(
                    context.Request.Url.LocalPath,
                    context.Request.HttpMethod);

                // Build the request.
                var request = new HttpRequest<TRouteInfo, TServerInfo>(
                    context,
                    routeInfo,
                    _serverInfo);

                // Invoke the middleware.
                foreach (var middlewareHandler in Middlewares)
                    await middlewareHandler(request);

                // Invoke the route handler and apply the result.
                var response = await handler(request);
                await response.Apply(context.Response);
            }
            catch
            {
                var response = new HttpResponse(HttpStatusCode.InternalServerError);
                await response.Apply(context.Response);
            }
        }
    }
}