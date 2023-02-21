using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace HttpServer
{
    internal class HttpRouter : IHttpRouter
    {
        private const string GLOBAL_MIDDLEWARE_KEY = "*";

        private readonly ILogger _logger;

        private readonly Dictionary<string, HttpController> _routingTable = new Dictionary<string, HttpController>();
        private readonly List<KeyValuePair<string, List<IMiddleware>>> _middlewareTable = new List<KeyValuePair<string, List<IMiddleware>>>();

        public HttpRouter(
            ILogger logger)
        {
            _logger = logger;
        }

        private List<IMiddleware> GetMiddlewaresForRoute(string route)
        {
            var middlewareEntry = _middlewareTable
                .FirstOrDefault(entry => entry.Key.Equals(route));

            if (middlewareEntry.Equals(default(KeyValuePair<string, List<IMiddleware>>)))
                return Enumerable.Empty<IMiddleware>().ToList();

            return middlewareEntry.Value;
        }

        private async Task InvokeMiddlewaresAsync(
            string route,
            HttpRequest req,
            HttpResponse res)
        {
            var middlewares = GetMiddlewaresForRoute(GLOBAL_MIDDLEWARE_KEY);

            middlewares.AddRange(
                GetMiddlewaresForRoute(route));

            middlewares.AddRange(
                GetMiddlewaresForRoute(route.TrimEnd('/')));

            foreach (var middleware in middlewares)
            {
                await middleware.HandleRequestAsync(
                    req, 
                    res);
            }
        }

        public async Task RouteAsync(HttpListenerContext ctx)
        {
            try
            {
                _logger.LogInformation($"{nameof(RouteAsync)} ENTER");

                var req = new HttpRequest(ctx.Request);
                var res = new HttpResponse(ctx.Response);

#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var route = ctx.Request.Url.LocalPath;
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                if (string.IsNullOrWhiteSpace(route))
                {
                    _logger.LogWarning($"'{nameof(route)}' cannot be null or whitespace.");

                    await res.AnswerWithStatusCodeAsync(HttpStatusCode.InternalServerError);
                    return;
                }

                route = route.ToLower();

                if (!_routingTable.TryGetValue(route, out var controller) && !_routingTable.TryGetValue(route.TrimEnd('/'), out controller))
                {
                    _logger.LogWarning($"Failed to resolve controller for route '{route}'.");

                    await res.AnswerWithStatusCodeAsync(HttpStatusCode.InternalServerError);
                    return;
                }

                await InvokeMiddlewaresAsync(
                    route,
                    req,
                    res);

                await controller.HandleAnyAsync(
                    req,
                    res);
            }
            finally
            {
                _logger.LogInformation($"{nameof(RouteAsync)} LEAVE");
            }
        }

        public void RegisterMiddleware<TMiddleware>(
            string route,
            TMiddleware middleware)

            where TMiddleware : class, IMiddleware
        {
            var entry = _middlewareTable.FirstOrDefault(entry => entry.Key.Equals(route));

            if (!entry.Equals(default(KeyValuePair<string, List<IMiddleware>>)))
            {
                entry.Value.Add(middleware);
                return;
            }

            entry = new KeyValuePair<string, List<IMiddleware>>(
                route,
                new List<IMiddleware> { middleware });

            _middlewareTable.Add(entry);
        }

        public void RegisterController<TController>(
            string route,
            TController controller,
            bool overrideExistingRoute = false)

            where TController : HttpController
        {
            try
            {
                _logger.LogInformation($"{nameof(RegisterController)} ENTER");

                route = route.ToLower();

                if (_routingTable.ContainsKey(route) && !overrideExistingRoute)
                    throw new ArgumentException($"There is already a controller registered for the route '{route}'!");

                _routingTable.Add(
                    route,
                    controller);
            }
            finally
            {
                _logger.LogInformation($"{nameof(RegisterController)} LEAVE");
            }
        }
    }
}
