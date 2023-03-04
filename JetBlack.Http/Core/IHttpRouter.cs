using System;
using System.Threading.Tasks;

namespace JetBlack.Http.Core
{
    public interface IHttpRouter<TRouteInfo, TServerInfo>
        where TRouteInfo : class
        where TServerInfo : class
    {
        void AddRoute(string path, Func<HttpRequest<TRouteInfo, TServerInfo>, Task<HttpResponse>> handler);
        (Func<HttpRequest<TRouteInfo, TServerInfo>, Task<HttpResponse>>, TRouteInfo) FindHandler(string path);
    }
}
