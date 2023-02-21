using System;

namespace HttpServer
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class RouteAttribute : Attribute
    {
        public string Path { get; }

        public RouteAttribute(string path)
        {
            Path = path;
        }
    }
}
