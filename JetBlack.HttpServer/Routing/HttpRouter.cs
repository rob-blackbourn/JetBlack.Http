using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace JetBlack.HttpServer.Routing
{
    internal class HttpRouter : IHttpRouter
    {
        private readonly ILogger<HttpRouter> _logger;

        private readonly List<Route> _routes = new List<Route>();

        public HttpRouter(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HttpRouter>();
        }


        private (Func<HttpRequest, Task<HttpResponse>>?, Dictionary<string, object?>?) FindRoute(string path)
        {
            foreach (var route in _routes)
            {
                var (isMatch, matches) = route.Path.Match(path);
                if (isMatch)
                    return (route.Controller, matches);
            }

            return (null, null);
        }

        public (Func<HttpRequest, Task<HttpResponse>>, Dictionary<string,object?>?) FindHandler(string path)
        {
            var (handler, matches) = FindRoute(path.ToLower());

            if (handler == null)
            {
                _logger.LogWarning($"Failed to resolve controller for route '{path}'.");
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
            try
            {
                _logger.LogInformation($"{nameof(AddRoute)} ENTER");

                var route = new Route(new PathDefinition(path.ToLower()), handler);
                _routes.Add(route);
            }
            finally
            {
                _logger.LogInformation($"{nameof(AddRoute)} LEAVE");
            }
        }
    }
}
