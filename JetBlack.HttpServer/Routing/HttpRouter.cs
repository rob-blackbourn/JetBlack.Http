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
        private readonly ILogger _logger;

        private readonly List<Route> _routes = new List<Route>();

        public HttpRouter(
            ILogger logger)
        {
            _logger = logger;
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

        public async Task RouteAsync(HttpRequest req, HttpResponse res, string path)
        {
            try
            {
                _logger.LogInformation($"{nameof(RouteAsync)} ENTER");

                if (string.IsNullOrWhiteSpace(path))
                {
                    _logger.LogWarning($"'{nameof(path)}' cannot be null or whitespace.");

                    await res.AnswerWithStatusCodeAsync(HttpStatusCode.InternalServerError);
                    return;
                }

                var (handler, matches) = FindRoute(path.ToLower());

                if (handler == null)
                {
                    _logger.LogWarning($"Failed to resolve controller for route '{path}'.");

                    await res.AnswerWithStatusCodeAsync(HttpStatusCode.InternalServerError);
                    return;
                }

                await handler(
                    req,
                    res);
            }
            finally
            {
                _logger.LogInformation($"{nameof(RouteAsync)} LEAVE");
            }
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
