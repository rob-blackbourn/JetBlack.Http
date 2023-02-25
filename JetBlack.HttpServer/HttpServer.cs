﻿using System;
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
        private readonly List<Func<HttpRequest, HttpResponse, Task>> _middlewares = new List<Func<HttpRequest, HttpResponse, Task>>();

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

        public HttpServer RegisterMiddleware(
            Func<HttpRequest, HttpResponse, Task> middleware)
        {
            if (middleware is null)
            {
                throw new ArgumentNullException(nameof(middleware));
            }

            _middlewares.Add(middleware);

            return this;
        }

        public HttpServer AddRoute(
            string path,
            Func<HttpRequest, HttpResponse, Task> handler)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException($"'{nameof(path)}' cannot be null or whitespace.", nameof(path));
            }

            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            _router.AddRoute(path, handler);

            return this;
        }

        private async Task InvokeMiddlewaresAsync(
            HttpRequest req,
            HttpResponse res)
        {
            foreach (var handler in _middlewares)
            {
                await handler(
                    req, 
                    res);
            }
        }

        private async Task HandleContext(HttpListenerContext context)
        {
            var req = new HttpRequest(context);
            var res = new HttpResponse(context);
            var path = context.Request.Url.LocalPath;
            try
            {
                await InvokeMiddlewaresAsync(
                    req,
                    res);

                var handler = _router.FindHandler(path);
                await handler(req, res);

            }
            catch
            {
                await res.AnswerWithStatusCodeAsync(HttpStatusCode.InternalServerError);
            }
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
            finally
            {
                _logger.LogInformation($"{nameof(RunAsync)} LEAVE");
            }
        }
    }
}