using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HttpServer
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly ILogger<HttpServer> _logger;

        private IHttpRouter _router;

        public HttpServer(
            ILoggerFactory? loggerFactory,
            HttpListener listener)
        {
            if (listener is null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (!listener.Prefixes.Any())
            {
                throw new ArgumentException($"'{nameof(listener.Prefixes)}' must contain at least one prefix.");
            }

            _listener = listener;

            loggerFactory ??= NullLoggerFactory.Instance;

            _logger = loggerFactory.CreateLogger<HttpServer>();
            _router = new HttpRouter(
                loggerFactory.CreateLogger<HttpRouter>());
        }

        public HttpServer(HttpListener listener)
            : this(loggerFactory: null, listener)
        {
        }

        public HttpServer(
            ILoggerFactory? loggerFactory,
            Func<HttpListener> listenerFactory)

            : this(loggerFactory, listenerFactory.Invoke())
        {
        }

        public HttpServer(Func<HttpListener> listenerFactory)
            : this(loggerFactory: null, listenerFactory.Invoke())
        {
        }

        public HttpServer(
            ILoggerFactory? loggerFactory,
            params string[] listenerPrefixes)

            : this(loggerFactory, CreateHttpListenerWithPrefixes(listenerPrefixes))
        {

        }

        public HttpServer(params string[] listenerPrefixes)
            : this(loggerFactory: null, listenerPrefixes)
        {
        }

        public HttpServer(
            ILoggerFactory? loggerFactory,
            HttpServerConfig configuration,
            IEnumerable<HttpController> controllers)

            : this(loggerFactory, CreateHttpListenerWithPrefixes(configuration.ListenerPrefixes))
        {
            foreach (var controller in controllers)
                RegisterController(controller);
        }

        public HttpServer(
            HttpServerConfig configuration,
            IEnumerable<HttpController> controllers)

            : this(loggerFactory: null, configuration, controllers: controllers)
        {
        }

        public HttpServer(
            ILoggerFactory? loggerFactory,
            HttpServerConfig configuration,
            IEnumerable<IMiddleware> middlewares)

            : this(loggerFactory, CreateHttpListenerWithPrefixes(configuration.ListenerPrefixes))
        {
            foreach (var middleware in middlewares)
                RegisterMiddleware(middleware);
        }

        public HttpServer(
            HttpServerConfig configuration,
            IEnumerable<IMiddleware> middlewares)

            : this(loggerFactory: null, configuration, middlewares: middlewares)
        {
        }

        public HttpServer(
            ILoggerFactory? loggerFactory,
            HttpServerConfig configuration,
            IEnumerable<HttpController> controllers,
            IEnumerable<IMiddleware> middlewares)

            : this(loggerFactory, CreateHttpListenerWithPrefixes(configuration.ListenerPrefixes))
        {
            foreach (var controller in controllers)
                RegisterController(controller);

            foreach (var middleware in middlewares)
                RegisterMiddleware(middleware);
        }

        public HttpServer(
            HttpServerConfig configuration,
            IEnumerable<HttpController> controllers,
            IEnumerable<IMiddleware> middlewares)

            : this(loggerFactory: null, configuration, controllers: controllers, middlewares: middlewares)
        {
        }

        private static HttpListener CreateHttpListenerWithPrefixes(IEnumerable<string> listenerPrefixes)
        {
            var listener = new HttpListener();

            foreach (var prefix in listenerPrefixes)
                listener.Prefixes.Add(prefix);

            return listener;
        }


        /// <summary>
        /// Allows to use a custom router implementation.
        /// </summary>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer UseRouter<TRouter>()
            where TRouter : class, IHttpRouter, new()
        {
            return UseRouter(new TRouter());
        }

        /// <summary>
        /// Allows to use a custom router implementation.
        /// </summary>
        /// <param name="router">The <see cref="IHttpRouter"/> instance to use</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer UseRouter<TRouter>(TRouter router)
            where TRouter : class, IHttpRouter
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            return this;
        }

        /// <summary>
        /// Wraps a <see cref="Func{T1, T2, TResult}"/> into a (internal) middleware class and registers it for the given route.
        /// </summary>
        /// <param name="handler">The <see cref="Func{T1, T2, TResult}"/> which should be wrapped into an (internal) middleware class.</param>
        /// <param name="route">The route on which the middleware should be registered.</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer RegisterMiddleware(
            Func<HttpRequest, HttpResponse, Task> handler,
            string route = "*")
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException($"'{nameof(route)}' cannot be null or whitespace.", nameof(route));
            }

            var middleware = new FuncAsMiddleware(handler);

            return RegisterMiddleware(
                middleware,
                route);
        }

        /// <summary>
        /// Allows to register a middleware used globally or bound to a specific route.
        /// </summary>
        /// <param name="route">The route on which the middleware should be registered.</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer RegisterMiddleware<TMiddleware>(string route = "*")
            where TMiddleware : class, IMiddleware, new()
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException($"'{nameof(route)}' cannot be null or whitespace.", nameof(route));
            }

            return RegisterMiddleware(
                new TMiddleware(),
                route);
        }

        /// <summary>
        /// Allows to register a middleware used globally or bound to a specific route.
        /// </summary>
        /// <param name="middleware"></param>
        /// <param name="route">The route on which the middleware should be registered.</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer RegisterMiddleware<TMiddleware>(
            TMiddleware middleware,
            string route = "*")

            where TMiddleware : class, IMiddleware
        {
            if (middleware is null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }

            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException($"'{nameof(route)}' cannot be null or whitespace.", nameof(route));
            }

            _router.RegisterMiddleware(
                route, 
                middleware);

            return this;
        }

        /// <summary>
        /// Constructs and registers a controller for the route declared with the <see cref="RouteAttribute"/>.
        /// </summary>
        /// <param name="overrideExistingRoute">Flag to indicate that a route (if already registered) should be overriden.</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer RegisterController<TController>(bool overrideExistingRoute = false)
            where TController : HttpController, new()
        {
            return RegisterController(
                new TController(), 
                overrideExistingRoute);
        }

        /// <summary>
        /// Registers a controller and extracts the route from the declared <see cref="RouteAttribute"/>.
        /// </summary>
        /// <param name="controller">A instance which must be assignable to the type <see cref="HttpController"/>.</param>
        /// <param name="overrideExistingRoute">Flag to indicate that a route (if already registered) should be overriden.</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer RegisterController<TController>(
            TController controller,
            bool overrideExistingRoute = false)

            where TController : HttpController
        {
            if (!TypeDiscovery.TryGetControllerRouteFromAttribute(typeof(TController), out var route))
                throw new InvalidOperationException($"Failed to resolve controller route!");

            return RegisterController(
                route,
                controller,
                overrideExistingRoute);
        }

        /// <summary>
        /// Wraps a <see cref="Func{T, TResult}"/> into a (internal) controller class and registers it for the given route.
        /// </summary>
        /// <param name="route">The route which should be handled by the controller.</param>
        /// <param name="handler">The <see cref="Func{T, TResult}"/> which should be wrapped into an (internal) controller class.</param>
        /// <param name="overrideExistingRoute">Flag to indicate that a route (if already registered) should be overriden.</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer RegisterController(
            string route,
            Func<HttpRequest, HttpResponse, Task> handler,
            bool overrideExistingRoute = false)
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException($"'{nameof(route)}' cannot be null or whitespace.", nameof(route));
            }

            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return RegisterController(
                route,
                new FuncAsHttpController(handler),
                overrideExistingRoute);
        }

        /// <summary>
        /// Constructs and registers a controller for the given route.
        /// </summary>
        /// <param name="route">The route which should be handled by the controller.</param>
        /// <param name="overrideExistingRoute">Flag to indicate that a route (if already registered) should be overriden.</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer RegisterController<TController>(
            string route,
            bool overrideExistingRoute = false)

            where TController : HttpController, new()
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException($"'{nameof(route)}' cannot be null or whitespace.", nameof(route));
            }

            return RegisterController(
                route, 
                new TController(), 
                overrideExistingRoute);
        }

        /// <summary>
        /// Registers a controller for the given route.
        /// </summary>
        /// <param name="route">The route which should be handled by the controller.</param>
        /// <param name="controller">A instance which must be assignable to the type parameter <typeparamref name="TController"/>.</param>
        /// <param name="overrideExistingRoute">Flag to indicate that a route (if already registered) should be overriden.</param>
        /// <returns>The current <see cref="HttpServer"/> instance.</returns>
        public HttpServer RegisterController<TController>(
            string route, 
            TController controller,
            bool overrideExistingRoute = false)

            where TController : HttpController
        {
            if (string.IsNullOrWhiteSpace(route))
            {
                throw new ArgumentException($"'{nameof(route)}' cannot be null or whitespace.", nameof(route));
            }

            if (controller is null)
            {
                throw new ArgumentNullException(nameof(controller));
            }

            _router.RegisterController(
                route,
                controller,
                overrideExistingRoute);

            return this;
        }


        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation($"{nameof(RunAsync)} ENTER");

                _listener.Start();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var ctx = await _listener.GetContextAsync();
                        await _router.RouteAsync(ctx);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, string.Empty);
                    }
                }

                _listener.Stop();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, null);
                throw;
            }
            finally
            {
                _logger.LogInformation($"{nameof(RunAsync)} LEAVE");
            }
        }
    }
}