using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBlack.Http.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace JetBlack.Http.Middleware
{
    public class CorsMiddleware<TRouteInfo, TServerInfo>
        where TRouteInfo : class
        where TServerInfo : class
    {
        private static readonly HashSet<string> ALL_METHODS = new HashSet<string>(new[] { "DELETE", "GET", "OPTIONS", "PATCH", "POST", "PUT" });

        private readonly ILogger<CorsMiddleware<TRouteInfo, TServerInfo>> _logger;
        private readonly ISet<string> _allowMethods;
        private readonly ISet<string>? _allowOrigins;
        private readonly ISet<string>? _allowHeaders;
        private readonly IDictionary<string, string> _corsHeaders = new Dictionary<string, string>();
        private readonly IDictionary<string, string> _preflightHeaders = new Dictionary<string, string>();
        private readonly bool _allowAllOrigins;
        private readonly bool _allowAllHeaders;
        private readonly Regex? _allowOriginRegex;

        /// <summary>
        /// Construct the CORS middleware.
        /// </summary>
        /// <param name="allowOrigins">A set of allowed origins, or null if all are allowed.</param>
        /// <param name="allowMethods">A set of allowed methods, or null if all are allowed.</param>
        /// <param name="allowHeaders">A set of allowed headers, or none if all are allowed.</param>
        /// <param name="allowCredentials">A boolean to control whether credentials are allowed.</param>
        /// <param name="allowOriginRegex">A regular expression to match with origins, or null if not required.</param>
        /// <param name="exposeHeaders">A set of headers to expose.</param>
        /// <param name="maxAge">The maximum age of the CORS headers.</param>
        /// <param name="loggerFactory">An optional logger factory.</param>
        public CorsMiddleware(
            ISet<string>? allowOrigins = null,
            ISet<string>? allowMethods = null,
            ISet<string>? allowHeaders = null,
            bool allowCredentials = false,
            string? allowOriginRegex = null,
            ISet<string>? exposeHeaders = null,
            int maxAge = 600,
            ILoggerFactory? loggerFactory = null)
        {
            _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<CorsMiddleware<TRouteInfo, TServerInfo>>();

            _allowMethods = allowMethods != null ? allowMethods : ALL_METHODS;

            _allowAllOrigins = allowOrigins == null;
            if (_allowAllOrigins)
            {
                _allowOrigins = null;
                _corsHeaders.Add("Access-Control-Allow-Origin", "*");
            }
            else
                _allowOrigins = allowOrigins;

            if (allowCredentials)
                _corsHeaders.Add("Access-Control-Allow-Credentials", "true");

            if (exposeHeaders != null)
                _corsHeaders.Add(
                    "Access-Control-Expose-Headers",
                    string.Join(", ", exposeHeaders));

            if (_allowAllOrigins)
                _preflightHeaders.Add("Access-Control-Allow-Origin", "*");
            else
                _preflightHeaders.Add("Vary", "Origin");

            _preflightHeaders.Add(
                "Access-Control-Allow-Methods",
                string.Join(", ", _allowMethods));
            _preflightHeaders.Add("Access-Control-Max-Age", maxAge.ToString());

            _allowAllHeaders = allowHeaders == null;
            if (allowHeaders == null)
                _allowHeaders = null;
            else
            {
                _allowHeaders = allowHeaders;
                _preflightHeaders.Add(
                    "Access-Control-Allow-Headers",
                    string.Join(", ", allowHeaders));
            }

            if (allowCredentials)
                _preflightHeaders.Add("Access-Control-Allow-Credentials", "true");

            _allowOriginRegex = allowOriginRegex == null
                ? null
                : new Regex(allowOriginRegex);
        }

        private HttpResponse PreflightCheck(NameValueCollection requestHeaders)
        {
            var responseHeaders = new WebHeaderCollection();
            foreach (var header in _preflightHeaders)
                responseHeaders[header.Key] = header.Value;

            try
            {
                var requestOrigin = requestHeaders["Origin"];
                if (IsAllowedOrigin(requestOrigin))
                {
                    if (!_allowAllOrigins)
                    {
                        // If self.allow_all_origins is True, then the "Access-Control-Allow-Origin"
                        // header is already set to "*".
                        // If we only allow specific origins, then we have to mirror back
                        // the Origin header in the response.
                        responseHeaders["Access-Control-Allow-Origin"] = requestOrigin;
                    }
                }
                else
                    throw new ApplicationException($"Invalid origin {requestOrigin}");

                var requestMethod = requestHeaders["Access-Control-Request-Method"];
                if (!_allowMethods.Contains(requestMethod))
                    throw new ApplicationException($"Invalid method {requestMethod}");

                // If we allow all headers, then we have to mirror back any requested
                // headers in the response.
                var accessControlRequestHeader = requestHeaders["Access-Control-Request-Headers"];
                if (accessControlRequestHeader != null)
                {
                    if (_allowAllHeaders)
                        responseHeaders["Access-Control-Allow-Headers"] = accessControlRequestHeader;
                    else if (_allowHeaders != null)
                        foreach (var name in accessControlRequestHeader.Split(',').Select(x => x.Trim()))
                            if (!_allowHeaders.Contains(name))
                                throw new ApplicationException($"Invalid header {name}");
                }

                _logger.LogDebug("Passed preflight checks");

                return HttpResponse.FromString("OK", 200, headers: responseHeaders);
            }
            catch (Exception error)
            {
                _logger.LogWarning(error, "Failed preflight checks with error %s");
                // We don't strictly need to use 400 responses here, since its up to
                // the browser to enforce the CORS policy, but its more informative
                // if we do.
                return HttpResponse.FromString(error.Message, 400);
            }
        }

        private bool IsAllowedOrigin(string origin)
        {
            if (_allowAllOrigins)
                return true;

            if (_allowOriginRegex != null && _allowOriginRegex.IsMatch(origin))
                return true;

            return _allowOrigins != null && _allowOrigins.Contains(origin);
        }

        private async Task<HttpResponse> CorsResponse(
                HttpRequest<TRouteInfo, TServerInfo> request,
                Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>> handler,
                CancellationToken token)
        {
            var requestHeaders = request.Context.Request.Headers;

            var response = await handler(request, token);

            var headers = response.Headers ?? new WebHeaderCollection();
            foreach (var header in _corsHeaders)
                headers[header.Key] = header.Value;

            var origin = requestHeaders["Origin"];
            if (origin == null)
                throw new ApplicationException("Origin cannot be null");

            // If request includes any cookie headers, then we must respond
            // with the specific origin instead of '*'.
            if (_allowAllOrigins && requestHeaders["Cookie"] != null)
                headers["Access-Control-Allow-Origin"] = origin;

            // If we only allow specific origins, then we have to mirror back
            // the Origin header in the response.
            else if (!_allowAllOrigins && IsAllowedOrigin(origin))
            {
                headers["Access-Control-Allow-Origin"] = origin;
                var varyValues = headers["Vary"];
                if (varyValues == null)
                    headers.Add("Vary", "Origin");
                else
                    headers["Vary"] = varyValues + ",Origin";
            }

            return new HttpResponse(
                response.StatusCode,
                headers: headers,
                body: response.Body);
        }

        /// <summary>
        /// Apply the middleware to a request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="handler">The handler to call.</param>
        /// <param name="token">A cancellation token.</param>
        /// <returns>The response decorated with CORS headers.</returns>
        public async Task<HttpResponse> Apply(
            HttpRequest<TRouteInfo, TServerInfo> request,
            Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>> handler,
            CancellationToken token)
        {
            // headers = header.to_dict(request.scope['headers'])

            if (request.Context.Request.Headers["Origin"] == null)
            {
                _logger.LogDebug("CORS processing skipped as there is no \"Origin\" header");
                return await handler(request, token);
            }

            if (request.Context.Request.HttpMethod == "OPTIONS" && request.Context.Request.Headers["Access-Control-Request-Method"] != null)
            {
                _logger.LogDebug("Performing preflight checks");
                return PreflightCheck(request.Context.Request.Headers);
            }

            _logger.LogDebug("Processing CORS response");
            return await CorsResponse(request, handler, token);
        }

        /// <summary>
        /// Create a CORS middleware handler.
        /// </summary>
        /// <param name="allowOrigins">A set of allowed origins, or null if all are allowed.</param>
        /// <param name="allowMethods">A set of allowed methods, or null if all are allowed.</param>
        /// <param name="allowHeaders">A set of allowed headers, or none if all are allowed.</param>
        /// <param name="allowCredentials">A boolean to control whether credentials are allowed.</param>
        /// <param name="allowOriginRegex">A regular expression to match with origins, or null if not required.</param>
        /// <param name="exposeHeaders">A set of headers to expose.</param>
        /// <param name="maxAge">The maximum age of the CORS headers.</param>
        /// <param name="loggerFactory">An optional logger factory.</param>
        /// <returns>A CORS middleware handler.</returns>
        public static Func<HttpRequest<TRouteInfo, TServerInfo>, Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>>, CancellationToken, Task<HttpResponse>> Create(
             ISet<string>? allowOrigins = null,
            ISet<string>? allowMethods = null,
            ISet<string>? allowHeaders = null,
            bool allowCredentials = false,
            string? allowOriginRegex = null,
            ISet<string>? exposeHeaders = null,
            int maxAge = 600,
            ILoggerFactory? loggerFactory = null)
        {
            var middleware = new CorsMiddleware<TRouteInfo, TServerInfo>();
            return middleware.Apply;
        }
    }
}