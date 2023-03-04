using System;
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
        /// <param name="path">The path.</param>
        /// <param name="handler">The handler.</param>
        void AddRoute(string path, Func<HttpRequest<TRouteInfo, TServerInfo>, Task<HttpResponse>> handler);
        (Func<HttpRequest<TRouteInfo, TServerInfo>, Task<HttpResponse>>, TRouteInfo) FindHandler(string path);
    }
}
