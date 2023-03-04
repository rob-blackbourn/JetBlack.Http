using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;

namespace JetBlack.Http.Rest
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;
    using RestRequestHandler = Func<HttpRequest<RestRouteInfo, RestServerInfo>, Task<HttpResponse>>;

    public class RestRouter : IHttpRouter<RestRouteInfo, RestServerInfo>
    {
        private static readonly (RestRequestHandler?, Dictionary<string, object?>) NoRoute = (null, new Dictionary<string, object?>());
        private static string[] defaultMethods = new[] { "GET" };

        private readonly ILogger<RestRouter> _logger;

        private readonly Dictionary<string, List<Route>> _routes = new Dictionary<string, List<Route>>();

        public bool IgnoreCase { get; set; }

        public RestRouter(bool ignoreCase, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RestRouter>();

            IgnoreCase = ignoreCase;
        }

        private (RestRequestHandler?, Dictionary<string, object?>) FindRoute(string path, string method)
        {
            if (!_routes.TryGetValue(method, out var methodRoutes))
                return NoRoute;

            foreach (var route in methodRoutes)
            {
                var (isMatch, matches) = route.Path.Match(path, IgnoreCase);
                if (isMatch)
                    return (route.Handler, matches);
            }

            return NoRoute;
        }

        public (RestRequestHandler, RestRouteInfo) FindHandler(string path, string method)
        {
            _logger.LogTrace($"Finding handler for route '{path}'.");
            var (handler, matches) = FindRoute(path, method);

            if (handler == null)
            {
                _logger.LogWarning($"Failed to find handler for route '{path}'.");
                return (NotFound, new RestRouteInfo(matches));
            }

            return (handler, new RestRouteInfo(matches));
        }

        private Task<HttpResponse> NotFound(RestRequest request)
        {
            return Task.FromResult(new HttpResponse(HttpStatusCode.NotFound));
        }

        public void AddRoute(
            Func<RestRequest, Task<HttpResponse>> handler,
            string path,
            params string[] methods)
        {
            _logger.LogDebug("Adding handler for {Path}", path);

            var route = new Route(new PathDefinition(path), handler);
            foreach (var method in methods.Length > 0 ? methods.Select(s => s.ToUpperInvariant()) : defaultMethods)
            {
                if (!_routes.TryGetValue(method, out var methodRoutes))
                    _routes.Add(method, methodRoutes = new List<Route>());
                methodRoutes.Add(route);
            }
        }
    }
}
