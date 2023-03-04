using System.Net;

namespace JetBlack.Http.Core
{
    public class HttpRequest<TRouteInfo, TServerInfo>
        where TRouteInfo : class
        where TServerInfo : class
    {
        public HttpListenerContext Context { get; }
        public HttpListenerRequest Request => Context.Request;
        public TRouteInfo RouteInfo { get; }
        public TServerInfo ServerInfo { get; }

        public HttpRequest(HttpListenerContext context, TRouteInfo routeInfo, TServerInfo serverInfo)
        {
            Context = context;
            RouteInfo = routeInfo;
            ServerInfo = serverInfo;
        }
    }
}