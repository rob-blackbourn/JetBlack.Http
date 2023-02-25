using System;
using System.Threading.Tasks;

namespace JetBlack.HttpServer.Routing
{
    public class Route
    {
        public PathDefinition Path { get; }
        public Func<HttpRequest, HttpResponse, Task> Controller { get; }

        public Route(PathDefinition path, Func<HttpRequest, HttpResponse, Task> controller)
        {
            Path = path;
            Controller = controller;
        }
    }
}