using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JetBlack.Http.Core
{
    public class HttpServer<TRouter, TRouteInfo>
        where TRouter : class, IHttpRouter<TRouteInfo>
        where TRouteInfo : class
    {
        private readonly ILogger<HttpServer<TRouter, TRouteInfo>> _logger;

        public HttpListener Listener { get; }
        public IList<Func<HttpRequest<TRouteInfo>, Task>> Middlewares { get; }
        public TRouter Router { get; }

        public HttpServer(
            Func<ILoggerFactory, TRouter> routerFactory,
            HttpListener? listener = null,
            IList<Func<HttpRequest<TRouteInfo>, Task>>? middlewares = null,
            ILoggerFactory? loggerFactory = null)
        {
            loggerFactory ??= NullLoggerFactory.Instance;
            _logger = loggerFactory.CreateLogger<HttpServer<TRouter, TRouteInfo>>();

            Listener = listener ?? new HttpListener();
            Middlewares = middlewares ?? new List<Func<HttpRequest<TRouteInfo>, Task>>();
            Router = routerFactory(loggerFactory);
        }

        private async Task InvokeMiddlewaresAsync(HttpRequest<TRouteInfo> request)
        {
            foreach (var handler in Middlewares)
                await handler(request);
        }

        private async Task HandleRequestAsync(HttpListenerContext context)
        {
            try
            {
                var (handler, routeInfo) = Router.FindHandler(context.Request.Url.LocalPath);
                var request = new HttpRequest<TRouteInfo>(context, routeInfo);

                await InvokeMiddlewaresAsync(request);
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