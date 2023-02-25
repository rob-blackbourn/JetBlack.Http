using System;
using System.Threading.Tasks;

namespace JetBlack.Http.Routing
{
    public class Route
    {
        public PathDefinition Path { get; }
        public Func<HttpRequest, Task<HttpResponse>> Controller { get; }

        public Route(PathDefinition path, Func<HttpRequest, Task<HttpResponse>> controller)
        {
            Path = path;
            Controller = controller;
        }
    }
}