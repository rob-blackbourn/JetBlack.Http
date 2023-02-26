using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using JetBlack.Http.Routing;

namespace JetBlack.Http
{
    public class HttpServer
    {
        private readonly ILogger<HttpServer> _logger;
        private readonly HttpListener _listener;
        private readonly List<Func<HttpRequest, Task>> _middlewares = new List<Func<HttpRequest, Task>>();

        public IHttpRouter Router { get; }

        public HttpServer(
            HttpListener listener,
            IHttpRouter? router = null,
            ILoggerFactory? loggerFactory = null)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<HttpServer>();

            if (listener is null)
                throw new ArgumentNullException(nameof(listener));

            if (!listener.Prefixes.Any())
                throw new ArgumentException($"'{nameof(listener.Prefixes)}' must contain at least one prefix.");

            _listener = listener;
            Router = router ?? new HttpRouter(loggerFactory);
        }

        public HttpServer(
            Func<HttpListener> listenerFactory,
            IHttpRouter? router = null,
            ILoggerFactory? loggerFactory = null)
            : this(listenerFactory.Invoke(), router, loggerFactory)
        {
        }

        public HttpServer(
            string[] listenerPrefixes,
            IHttpRouter? router = null,
            ILoggerFactory? loggerFactory = null)
            : this(
                CreateHttpListenerWithPrefixes(listenerPrefixes),
                router,
                loggerFactory)
        {
        }

        private static HttpListener CreateHttpListenerWithPrefixes(IEnumerable<string> listenerPrefixes)
        {
            var listener = new HttpListener();

            foreach (var prefix in listenerPrefixes)
                listener.Prefixes.Add(prefix);

            return listener;
        }

        public HttpServer AddMiddleware(Func<HttpRequest, Task> middleware)
        {
            if (middleware is null)
                throw new ArgumentNullException(nameof(middleware));

            _middlewares.Add(middleware);

            return this;
        }

        public HttpServer AddRoute(string path, Func<HttpRequest, Task<HttpResponse>> handler)
        {
            Router.AddRoute(path, handler);

            return this; // This is a convenience method for fluid style calls.
        }

        private async Task InvokeMiddlewaresAsync(HttpRequest request)
        {
            foreach (var handler in _middlewares)
                await handler(request);
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            var request = new HttpRequest(context);
            var path = context.Request.Url.LocalPath;
            try
            {
                await InvokeMiddlewaresAsync(request);

                var (handler, matches) = Router.FindHandler(path);
                request.Matches = matches;
                var response = await handler(request);
                await response.Apply(context.Response);
            }
            catch
            {
                var response = await InternalServerError();
                await response.Apply(context.Response);
            }
        }

        private Task<HttpResponse> InternalServerError()
        {
            return Task.FromResult(new HttpResponse(HttpStatusCode.InternalServerError));
        }

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _listener.Start();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        var context = await _listener.GetContextAsync();
                        await HandleRequestAsync(context);
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
        }
    }
}