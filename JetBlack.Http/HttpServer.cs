﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JetBlack.Http
{
    public class HttpServer
    {
        private readonly ILogger<HttpServer> _logger;

        public HttpListener Listener { get; }
        public IList<Func<HttpRequest, Task>> Middlewares { get; }
        public IHttpRouter Router { get; }

        public HttpServer(
            HttpListener? listener = null,
            IHttpRouter? router = null,
            IList<Func<HttpRequest, Task>>? middlewares = null,
            ILoggerFactory? loggerFactory = null)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<HttpServer>();

            Listener = listener ?? new HttpListener();
            Middlewares = middlewares ?? new List<Func<HttpRequest, Task>>();
            Router = router ?? new HttpRouter(true, loggerFactory);
        }

        public HttpServer(
            Func<HttpListener> listenerFactory,
            IHttpRouter? router = null,
            IList<Func<HttpRequest, Task>>? middlewares = null,
            ILoggerFactory? loggerFactory = null)
            : this(listenerFactory.Invoke(), router, middlewares, loggerFactory)
        {
        }

        public HttpServer(
            string[] listenerPrefixes,
            IHttpRouter? router = null,
            IList<Func<HttpRequest, Task>>? middlewares = null,
            ILoggerFactory? loggerFactory = null)
            : this(
                CreateHttpListener(listenerPrefixes),
                router,
                middlewares,
                loggerFactory)
        {
        }

        private static HttpListener CreateHttpListener(
            IEnumerable<string> listenerPrefixes)
        {
            var listener = new HttpListener();

            foreach (var prefix in listenerPrefixes)
                listener.Prefixes.Add(prefix);

            return listener;
        }

        private async Task InvokeMiddlewaresAsync(HttpRequest request)
        {
            foreach (var handler in Middlewares)
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
            return Task.FromResult(
                new HttpResponse(HttpStatusCode.InternalServerError));
        }

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
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "The listener has faulted.");
                throw;
            }

            _logger.LogInformation("Stopped.");
        }
    }
}