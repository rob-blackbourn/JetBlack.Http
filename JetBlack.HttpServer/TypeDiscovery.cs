using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace JetBlack.HttpServer
{
    public static class TypeDiscovery
    {
        private static readonly Type _middlewareType = typeof(IMiddleware);

        public static IEnumerable<Type> FindMiddlewareTypes(Assembly assembly)
        {
            return FindTypes(assembly, _middlewareType);
        }

        private static IEnumerable<Type> FindTypes(
            Assembly assembly,
            Type type)
        {
            return assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(type));
        }

        public static bool TryGetControllerRouteFromAttribute(Type type, out string path)
        {
            path = string.Empty;

            var attribute = type.GetCustomAttribute<RouteAttribute>();
            var attributePath = attribute?.Path?.ToLower();

            if (attributePath != null && !string.IsNullOrWhiteSpace(attributePath))
                path = attributePath;

            return string.IsNullOrWhiteSpace(path);
        }

        public static bool IsAssignableTo(this Type sourceType, Type? targetType)
            => targetType == null ? false : targetType.IsAssignableFrom(sourceType);
    }
}
