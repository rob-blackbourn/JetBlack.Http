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
        private readonly ISet<string> _allow_methods;
        private readonly ISet<string>? _allow_origins;
        private readonly ISet<string>? _allow_headers;
        private readonly IDictionary<string, string> _simple_headers = new Dictionary<string, string>();
        private readonly IDictionary<string, string> _preflight_headers = new Dictionary<string, string>();
        private readonly bool _allow_all_origins;
        private readonly bool _allow_all_headers;
        private readonly Regex? _allow_origin_regex;

        public CorsMiddleware(
            ISet<string>? allow_origins = null,
            ISet<string>? allow_methods = null,
            ISet<string>? allow_headers = null,
            bool allow_credentials = false,
            string? allow_origin_regex = null,
            ISet<string>? expose_headers = null,
            int max_age = 600,
            ILoggerFactory? loggerFactory = null)
        {
            _logger = (loggerFactory ?? NullLoggerFactory.Instance).CreateLogger<CorsMiddleware<TRouteInfo, TServerInfo>>();

            _allow_methods = allow_methods != null ? allow_methods : ALL_METHODS;

            _allow_all_origins = allow_origins == null;
            if (_allow_all_origins)
            {
                _allow_origins = null;
                _simple_headers.Add("Access-Control-Allow-Origin", "*");
            }
            else
                _allow_origins = allow_origins;

            if (allow_credentials)
                _simple_headers.Add("Access-Control-Allow-Credentials", "true");

            if (expose_headers != null)
                _simple_headers.Add("Access-Control-Expose-Headers", string.Join(", ", expose_headers));

            if (_allow_all_origins)
                _preflight_headers.Add("Access-Control-Allow-Origin", "*");
            else
                _preflight_headers.Add("Vary", "Origin");

            _preflight_headers.Add("Access-Control-Allow-Methods", string.Join(", ", _allow_methods));
            _preflight_headers.Add("Access-Control-Max-Age", max_age.ToString());

            _allow_all_headers = allow_headers != null;
            if (allow_headers == null)
                _allow_headers = null;
            else
            {
                _allow_headers = allow_headers;
                _preflight_headers.Add("Access-Control-Allow-Headers", string.Join(", ", allow_headers));
            }

            if (allow_credentials)
                _preflight_headers.Add("Access-Control-Allow-Credentials", "true");

            _allow_origin_regex = allow_origin_regex == null ? null : new Regex(allow_origin_regex);
        }

        private HttpResponse PreflightCheck(NameValueCollection request_header_map)
        {
            var response_headers = new WebHeaderCollection();
            foreach (var (key, value) in _preflight_headers)
                response_headers[key] = value;

            try
            {
                var requested_origin = request_header_map["ORIGIN"];
                if (IsAllowedOrigin(requested_origin))
                {
                    if (!_allow_all_origins)
                        // If self.allow_all_origins is True, then the "Access-Control-Allow-Origin"
                        // header is already set to "*".
                        // If we only allow specific origins, then we have to mirror back
                        // the Origin header in the response.
                        response_headers["Access-Control-Allow-Origin"] = requested_origin;
                }
                else
                    throw new ApplicationException($"Invalid origin {requested_origin}");

                var requested_method = request_header_map["Access-Control-Request-Method"];
                if (!_allow_methods.Contains(requested_method))
                    throw new ApplicationException($"Invalid method {requested_method}");

                // If we allow all headers, then we have to mirror back any requested
                // headers in the response.
                var access_control_request_header = request_header_map["Access-Control-Request-Headers"];
                if (access_control_request_header != null)
                {
                    if (_allow_all_headers)
                        response_headers["Access-Control-Allow-Headers"] = access_control_request_header;
                    else if (_allow_headers != null)
                        foreach (var hdr in access_control_request_header.Split(",").Select(x => x.Trim()))
                            if (!_allow_headers.Contains(hdr))
                                throw new ApplicationException($"Invalid header {hdr}");
                }

                _logger.LogDebug("Passed preflight checks");

                return HttpResponse.FromString("OK", HttpStatusCode.OK, headers: response_headers);
            }
            catch (Exception error)
            {
                _logger.LogWarning(error, "Failed preflight checks with error %s");
                // We don't strictly need to use 400 responses here, since its up to
                // the browser to enforce the CORS policy, but its more informative
                // if we do.
                return HttpResponse.FromString(error.Message, HttpStatusCode.BadRequest);
            }
        }

        private bool IsAllowedOrigin(string origin)
        {
            if (_allow_all_origins)
                return true;

            if (_allow_origin_regex != null && _allow_origin_regex.IsMatch(origin))
                return true;

            return _allow_origins != null && _allow_origins.Contains(origin);
        }

        private async Task<HttpResponse> SimpleResponse(
                HttpRequest<TRouteInfo, TServerInfo> request,
                Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>> handler,
                CancellationToken token)
        {
            var request_headers = request.Context.Request.Headers;

            var response = await handler(request, token);

            var headers = response.Headers ?? new WebHeaderCollection();
            foreach (var (key, value) in _simple_headers)
                headers[key] = value;

            var origin = request_headers["Origin"];
            if (origin == null)
                throw new ApplicationException("Origin cannot be null");

            // If request includes any cookie headers, then we must respond
            // with the specific origin instead of '*'.
            if (_allow_all_origins && request_headers["Cookie"] != null)
                headers["Access-Control-Allow-Origin"] = origin;

            // If we only allow specific origins, then we have to mirror back
            // the Origin header in the response.
            else if (!_allow_all_origins && IsAllowedOrigin(origin))
            {
                headers["Access-Control-Allow-Origin"] = origin;
                var vary_values = headers["Vary"];
                if (vary_values == null)
                    headers.Add("Vary", "Origin");
                else
                    headers["Vary"] = vary_values + ",Origin";
            }

            return new HttpResponse(
                response.StatusCode,
                headers: headers,
                body: response.Body);
        }

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

            _logger.LogDebug("Processing simple response");
            return await SimpleResponse(request, handler, token);
        }
    }
}