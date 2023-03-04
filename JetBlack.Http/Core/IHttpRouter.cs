using System;
using System.Threading.Tasks;

namespace JetBlack.Http.Core
{
    public interface IHttpRouter<TRouteInfo> where TRouteInfo : class
    {
        void AddRoute(string path, Func<HttpRequest<TRouteInfo>, Task<HttpResponse>> handler);
        (Func<HttpRequest<TRouteInfo>, Task<HttpResponse>>, TRouteInfo?) FindHandler(string path);
    }
}
