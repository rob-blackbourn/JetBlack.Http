using System.Net;

namespace JetBlack.Http.Core
{
    public class HttpRequest<TRouteInfo> where TRouteInfo : class
    {
        public HttpListenerContext Context { get; }
        public HttpListenerRequest Request => Context.Request;
        public TRouteInfo? RouteInfo { get; }

        public HttpRequest(HttpListenerContext context, TRouteInfo? routeInfo)
        {
            Context = context;
            RouteInfo = routeInfo;
        }
    }
}