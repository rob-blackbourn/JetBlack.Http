using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;

namespace JetBlack.Http.Rest
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;
    using RestRequestHandler = Func<HttpRequest<RestRouteInfo, RestServerInfo>, CancellationToken, Task<HttpResponse>>;

    public class RestRouter : IHttpRouter<RestRouteInfo, RestServerInfo>
    {
        private static readonly (RestRequestHandler?, Dictionary<string, object?>) NoRoute = (null, new Dictionary<string, object?>());
        private static string[] defaultMethods = new[] { "GET" };

        private readonly ILogger<RestRouter> _logger;

        private readonly Dictionary<string, List<Route>> _routes = new Dictionary<string, List<Route>>();

        /// <summary>
        /// If true the case of path elements should be ignored.
        /// </summary>
        /// <value>True if the case of path elements should be ignored.</value>
        public bool IgnoreCase { get; set; }

        /// <summary>
        /// Constructs a rest router.
        /// </summary>
        /// <param name="ignoreCase">Whether to ignore case.</param>
        /// <param name="loggerFactory">The logger factory.</param>
        public RestRouter(bool ignoreCase, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RestRouter>();

            IgnoreCase = ignoreCase;
        }

        /// <inheritdoc/>
        public void AddRoute(
            RestRequestHandler handler,
            string path,
            params string[] methods)
        {
            _logger.LogDebug("Adding handler for {Path}", path);

            lock (_routes)
            {
                var route = new Route(new PathDefinition(path), handler);
                foreach (var method in methods.Length > 0 ? methods.Select(s => s.ToUpperInvariant()) : defaultMethods)
                {
                    if (!_routes.TryGetValue(method, out var methodRoutes))
                        _routes.Add(method, methodRoutes = new List<Route>());
                    methodRoutes.Add(route);
                }
            }
        }

        /// <inheritdoc/>
        public (RestRequestHandler, RestRouteInfo) FindHandler(string path, string method)
        {
            _logger.LogTrace($"Finding handler for route '{path}'.");

            lock (_routes)
            {
                var (handler, matches) = FindRoute(path, method);

                if (handler == null)
                {
                    _logger.LogWarning($"Failed to find handler for route '{path}'.");
                    return (NotFound, new RestRouteInfo(matches));
                }

                return (handler, new RestRouteInfo(matches));
            }
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

        private Task<HttpResponse> NotFound(RestRequest request, CancellationToken token)
        {
            return Task.FromResult(new HttpResponse(404));
        }
    }
}
