using System;
using System.Threading.Tasks;

using JetBlack.Http.Core;

namespace JetBlack.Http.Rest
{
    public class Route
    {
        public PathDefinition Path { get; }
        public Func<HttpRequest<RestRouteInfo>, Task<HttpResponse>> Handler { get; }

        public Route(
            PathDefinition path,
            Func<HttpRequest<RestRouteInfo>, Task<HttpResponse>> handler)
        {
            Path = path;
            Handler = handler;
        }

        public override string ToString() => Path.ToString();
    }
}