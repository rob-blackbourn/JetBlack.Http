using System;

namespace Example
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RouteAttribute : Attribute
    {
        public string Path { get; }
        public string[] Methods { get; }

        public RouteAttribute(string path, params string[] methods)
        {
            Path = path;
            Methods = methods;
        }
    }
}