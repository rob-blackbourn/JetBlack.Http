using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using JetBlack.HttpServer.Routing;

namespace JetBlack.HttpServer
{
    public class HttpServer
    {
        private readonly HttpListener _listener;
        private readonly ILogger<HttpServer> _logger;

        private IHttpRouter _router;
        private readonly List<Func<HttpRequest, Task>> _middlewares = new List<Func<HttpRequest, Task>>();

        public HttpServer(
            HttpListener listener,
            ILoggerFactory? loggerFactory = null)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<HttpServer>();

            if (listener is null)
                throw new ArgumentNullException(nameof(listener));

            if (!listener.Prefixes.Any())
                throw new ArgumentException($"'{nameof(listener.Prefixes)}' must contain at least one prefix.");

            _listener = listener;
            _router = new HttpRouter(loggerFactory);
        }

        public HttpServer(Func<HttpListener> listenerFactory, ILoggerFactory? loggerFactory = null)
            : this(listenerFactory.Invoke(), loggerFactory)
        {
        }

        public HttpServer(
            string[] listenerPrefixes,
            ILoggerFactory? loggerFactory = null)
            : this(CreateHttpListenerWithPrefixes(listenerPrefixes), loggerFactory)
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
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));

            if (handler is null)
                throw new ArgumentNullException(nameof(handler));

            _router.AddRoute(path, handler);

            return this;
        }

        private async Task InvokeMiddlewaresAsync(HttpRequest request)
        {
            foreach (var handler in _middlewares)
                await handler(request);
        }

        private async Task HandleContext(HttpListenerContext context)
        {
            var request = new HttpRequest(context);
            var path = context.Request.Url.LocalPath;
            try
            {
                await InvokeMiddlewaresAsync(request);

                var (handler, matches) = _router.FindHandler(path);
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
                        await HandleContext(context);
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