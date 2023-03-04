using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;

namespace JetBlack.Http.Rest
{
    public class RestServer : HttpServer<RestRouter, RestRouteInfo, RestServerInfo>
    {
        public RestServer(
            HttpListener? listener = null,
            RestRouter? router = null,
            IList<Func<HttpRequest<RestRouteInfo, RestServerInfo>, Task>>? middlewares = null,
            ILoggerFactory? loggerFactory = null)
            : base(
                lf => router ?? new RestRouter(true, lf),
                new RestServerInfo(),
                listener,
                middlewares,
                loggerFactory)
        {
        }
    }
}