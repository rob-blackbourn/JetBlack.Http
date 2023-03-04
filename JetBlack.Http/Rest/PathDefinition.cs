using System;
using System.Collections.Generic;
using System.Linq;

namespace JetBlack.Http.Rest
{
    internal class PathDefinition
    {
        private static (bool, Dictionary<string, object?>) NoMatch = (false, new Dictionary<string, object?>());

        private readonly List<PathSegment> _segments = new List<PathSegment>();

        public string Path { get; }
        public bool EndsWithSlash { get; }

        public PathDefinition(string path)
        {
            Path = path;

            if (!path.StartsWith("/"))
                throw new Exception("Paths must be absolute");
            // Trim off the leading '/'
            path = path.Substring(1);

            // Handle paths that end with a '/'
            if (path.EndsWith("/"))
            {
                path = path.Substring(0, path.Length - 2);
                EndsWithSlash = true;
            }
            else
                EndsWithSlash = false;

            // Parse each path segment.
            foreach (var segment in path.Split('/'))
                _segments.Add(new PathSegment(segment));
        }

        public (bool, Dictionary<string, object?>) Match(string path, bool ignoreCase)
        {
            if (!path.StartsWith("/"))
                throw new Exception("Paths must be absolute");

            // Handle trailing slash
            if (path.Substring(1).EndsWith("/") && _segments.Last().Type != "path")
            {
                if (!EndsWithSlash)
                    return NoMatch;
                path = path.Substring(0, path.Length - 1);
            }
            else if (EndsWithSlash)
                return NoMatch;

            var parts = path.Substring(1).Split('/');

            // Must have at least the same number of segments.
            if (parts.Length < _segments.Count)
                return NoMatch;

            // Keep the matches we find.
            var matches = new Dictionary<string, object?>();

            // A path with more segments is allowed if the last segment is a
            // variable of type 'path'.
            if (parts.Length > _segments.Count)
            {
                var lastSegment = _segments.Last();
                if (lastSegment.Type != "path")
                    return NoMatch;
                var index = _segments.Count - 1;
                matches[lastSegment.Name] = string.Join("/", parts.Skip(index));
                parts = parts.Take(index).ToArray();
            }

            // Now the path parts and segments are the same length we can check them.
            foreach (var item in parts.Zip(_segments, (a, b) => new { Part = a, Segment = b }))
            {
                var (isMatch, name, value) = item.Segment.Match(item.Part, ignoreCase);
                if (!isMatch)
                    return NoMatch;
                if (name != null)
                    matches[name] = value;
            }

            return (true, matches);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override string ToString() => Path;
    }
}