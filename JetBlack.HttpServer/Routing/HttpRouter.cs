using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace JetBlack.HttpServer.Routing
{
    internal class HttpRouter : IHttpRouter
    {
        private const string GLOBAL_MIDDLEWARE_KEY = "*";

        private readonly ILogger _logger;

        private readonly List<Route> _routes = new List<Route>();
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

        private (Func<HttpRequest, HttpResponse, Task>?, Dictionary<string, object?>?) FindRoute(string path)
        {
            foreach (var route in _routes)
            {
                var (isMatch, matches) = route.Path.Match(path);
                if (isMatch)
                    return (route.Controller, matches);
            }

            return (null, null);
        }

        public async Task RouteAsync(HttpListenerContext ctx)
        {
            try
            {
                _logger.LogInformation($"{nameof(RouteAsync)} ENTER");

                var req = new HttpRequest(ctx.Request);
                var res = new HttpResponse(ctx.Response);

                var path = ctx.Request.Url.LocalPath;

                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogWarning($"'{nameof(path)}' cannot be null or whitespace.");

                    await res.AnswerWithStatusCodeAsync(HttpStatusCode.InternalServerError);
                    return;
                }

                var (controller, matches) = FindRoute(path.ToLower());

                if (controller == null)
                {
                    _logger.LogWarning($"Failed to resolve controller for route '{path}'.");

                    await res.AnswerWithStatusCodeAsync(HttpStatusCode.InternalServerError);
                    return;
                }

                await InvokeMiddlewaresAsync(
                    path,
                    req,
                    res);

                await controller(
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

        public void RegisterController(
            string path,
            Func<HttpRequest, HttpResponse, Task> handler)
        {
            try
            {
                _logger.LogInformation($"{nameof(RegisterController)} ENTER");

                var route = new Route(new PathDefinition(path.ToLower()), handler);
                _routes.Add(route);
            }
            finally
            {
                _logger.LogInformation($"{nameof(RegisterController)} LEAVE");
            }
        }
    }
}
