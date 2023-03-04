using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;

namespace JetBlack.Http.Rest
{
    public class RestRouter : IHttpRouter<RestRouteInfo>
    {
        private readonly ILogger<RestRouter> _logger;

        private readonly List<Route> _routes = new List<Route>();

        public bool IgnoreCase { get; set; }

        public RestRouter(bool ignoreCase, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RestRouter>();

            IgnoreCase = ignoreCase;
        }


        private (Func<HttpRequest<RestRouteInfo>, Task<HttpResponse>>?, Dictionary<string, object?>?) FindRoute(string path)
        {
            foreach (var route in _routes)
            {
                var (isMatch, matches) = route.Path.Match(path, IgnoreCase);
                if (isMatch)
                    return (route.Handler, matches);
            }

            return (null, null);
        }

        public (Func<HttpRequest<RestRouteInfo>, Task<HttpResponse>>, RestRouteInfo?) FindHandler(string path)
        {
            _logger.LogTrace($"Finding handler for route '{path}'.");
            var (handler, matches) = FindRoute(path);

            if (handler == null)
            {
                _logger.LogWarning($"Failed to find handler for route '{path}'.");
                return (NotFound, null);
            }

            return (handler, new RestRouteInfo(matches));
        }

        private Task<HttpResponse> NotFound(HttpRequest<RestRouteInfo> request)
        {
            return Task.FromResult(new HttpResponse(HttpStatusCode.NotFound));
        }

        public void AddRoute(
            string path,
            Func<HttpRequest<RestRouteInfo>, Task<HttpResponse>> handler)
        {
            _logger.LogDebug("Adding handler for {Path}", path);

            var route = new Route(new PathDefinition(path), handler);
            _routes.Add(route);
        }

    }
}
