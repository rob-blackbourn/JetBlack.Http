using System;
using System.Threading;
using System.Threading.Tasks;

namespace JetBlack.Http.Core
{
    /// <summary>
    /// The interface for HTTP routers.
    /// </summary>
    /// <typeparam name="TRouteInfo">The route information which exists for the lifetime of the invocation of the route.</typeparam>
    /// <typeparam name="TServerInfo">The server information which exists for the lifetime of the server.</typeparam>
    public interface IHttpRouter<TRouteInfo, TServerInfo>
        where TRouteInfo : class
        where TServerInfo : class
    {
        /// <summary>
        /// Add a route.
        /// </summary>
        /// <param name="handler">The handler.</param>
        /// <param name="path">The path.</param>
        /// <param name="methods">The HTTP methods.</param>
        void AddRoute(
            Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>> handler,
            string path,
            params string[] methods);

        /// <summary>
        /// Find a handler for a route. If no handler is found return an appropriate error handler.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="method">The HTTP method</param>
        /// <returns></returns>
        (Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>>, TRouteInfo) FindHandler(
            string path,
            string method);
    }
}
