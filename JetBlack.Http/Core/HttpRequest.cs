using System.Net;

namespace JetBlack.Http.Core
{
    /// <summary>
    /// The HTTP request passed to handlers.
    /// </summary>
    /// <typeparam name="TRouteInfo">Route information which exists for the lifetime of the invocation of the route.</typeparam>
    /// <typeparam name="TServerInfo">Server information which exists for the lifetime of the server.</typeparam>
    public class HttpRequest<TRouteInfo, TServerInfo>
        where TRouteInfo : class
        where TServerInfo : class
    {
        /// <summary>
        /// The context.
        /// </summary>
        /// <value>The context provider by the listener,</value>
        public HttpListenerContext Context { get; }
        /// <summary>
        /// The Request.
        /// </summary>
        public TRouteInfo RouteInfo { get; }
        /// <summary>
        /// Server information. This is valid for the lifetime of the server.
        /// </summary>
        /// <value>Server information</value>
        public TServerInfo ServerInfo { get; }

        /// <summary>
        /// Constructs an HTTP request.
        /// </summary>
        /// <param name="context">The listener context.</param>
        /// <param name="routeInfo">The route information.</param>
        /// <param name="serverInfo">The server information.</param>
        public HttpRequest(HttpListenerContext context, TRouteInfo routeInfo, TServerInfo serverInfo)
        {
            Context = context;
            RouteInfo = routeInfo;
            ServerInfo = serverInfo;
        }
    }
}