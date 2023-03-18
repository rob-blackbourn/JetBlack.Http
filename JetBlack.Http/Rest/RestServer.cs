using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using System.Threading;

namespace JetBlack.Http.Rest
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;

    /// <summary>
    /// A REST server.
    /// </summary>
    public class RestServer : HttpServer<RestRouter, RestRouteInfo, RestServerInfo>
    {
        /// <summary>
        /// Construct a REST server.
        /// </summary>
        /// <param name="listener">An optional listener.</param>
        /// <param name="router">An optional router.</param>
        /// <param name="middlewares">Optional middlewares.</param>
        /// <param name="startupHandlers">Optional startup handlers</param>
        /// <param name="shutdownHandlers">Optional shutdown handlers</param>
        /// <param name="loggerFactory">An optional logger factory.</param>
        public RestServer(
            HttpListener? listener = null,
            RestRouter? router = null,
            IList<
                Func<
                    HttpRequest<RestRouteInfo, RestServerInfo>,
                    Func<HttpRequest<RestRouteInfo, RestServerInfo>, CancellationToken, Task<HttpResponse>>,
                    CancellationToken,
                    Task<HttpResponse>>>? middlewares = null,
            IList<Func<RestServerInfo, CancellationToken, Task>>? startupHandlers = null,
            IList<Func<RestServerInfo, Task>>? shutdownHandlers = null,
            ILoggerFactory? loggerFactory = null)
            : base(
                lf => router ?? new RestRouter(true, lf),
                new RestServerInfo(),
                listener,
                middlewares,
                startupHandlers,
                shutdownHandlers,
                loggerFactory)
        {
        }

        public RestServer(ILoggerFactory loggerFactory)
            : this(null, null, null, null, null, loggerFactory)
        {
        }
    }
}