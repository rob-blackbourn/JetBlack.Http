using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;

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
        /// <param name="loggerFactory">An optional logger factory.</param>
        public RestServer(
            HttpListener? listener = null,
            RestRouter? router = null,
            IList<Func<RestRequest, Task>>? middlewares = null,
            ILoggerFactory? loggerFactory = null)
            : base(
                lf => router ?? new RestRouter(true, lf),
                new RestServerInfo(),
                listener,
                middlewares,
                loggerFactory)
        {
        }

        public RestServer(ILoggerFactory loggerFactory)
            : this(null, null, null, loggerFactory)
        {
        }
    }
}