using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace JetBlack.Http
{
    public class HttpRouter : IHttpRouter
    {
        private readonly ILogger<HttpRouter> _logger;

        private readonly List<Route> _routes = new List<Route>();

        public bool IgnoreCase { get; set; }

        public HttpRouter(bool ignoreCase, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpRouter>();

            IgnoreCase = ignoreCase;
        }


        private (Func<HttpRequest, Task<HttpResponse>>?, Dictionary<string, object?>?) FindRoute(string path)
        {
            foreach (var route in _routes)
            {
                var (isMatch, matches) = route.Path.Match(path, IgnoreCase);
                if (isMatch)
                    return (route.Handler, matches);
            }

            return (null, null);
        }

        public (Func<HttpRequest, Task<HttpResponse>>, Dictionary<string, object?>?) FindHandler(string path)
        {
            _logger.LogTrace($"Finding handler for route '{path}'.");
            var (handler, matches) = FindRoute(path);

            if (handler == null)
            {
                _logger.LogWarning($"Failed to find handler for route '{path}'.");
                return (NotFound, null);
            }

            return (handler, matches);
        }

        private Task<HttpResponse> NotFound(HttpRequest request)
        {
            return Task.FromResult(new HttpResponse(HttpStatusCode.NotFound));
        }

        public void AddRoute(
            string path,
            Func<HttpRequest, Task<HttpResponse>> handler)
        {
            _logger.LogDebug("Adding handler for {Path}", path);

            var route = new Route(new PathDefinition(path), handler);
            _routes.Add(route);
        }
    }
}
