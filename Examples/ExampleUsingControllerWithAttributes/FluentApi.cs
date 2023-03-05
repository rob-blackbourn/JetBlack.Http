using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using JetBlack.Http.Core;

namespace Example
{
    public static class FluentApi
    {
        public static HttpServer<TRouter, TRouteInfo, TServerInfo> AddController<TRouter, TRouteInfo, TServerInfo, TController>(
            this HttpServer<TRouter, TRouteInfo, TServerInfo> server,
            TController controller)
            where TRouter : class, IHttpRouter<TRouteInfo, TServerInfo>
            where TRouteInfo : class
            where TServerInfo : class
            where TController : class
        {
            foreach (var method in controller.GetType().GetMethods())
            {
                var attribute = method.GetCustomAttribute<RouteAttribute>();
                if (attribute != null)
                    server.AddRoute(
                        method.CreateDelegate<Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>>>(controller),
                        attribute.Path,
                        attribute.Methods);
            }

            return server;
        }
    }
}