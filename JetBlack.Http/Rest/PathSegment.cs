using System;
using System.Collections.Generic;
using System.Globalization;

namespace JetBlack.Http.Rest
{
    internal class PathSegment
    {
        private static readonly IDictionary<string, Func<string, string?, object?>> _converters = new Dictionary<string, Func<string, string?, object?>>
        {
            { "string", (value, format) => value },
            { "int", (value, format) => int.Parse(value) },
            { "double", (value, format) => double.Parse(value) },
            { "datetime", (value, format) => format == null ? DateTime.Parse(value) : DateTime.ParseExact(value, format, CultureInfo.InvariantCulture) },
            { "path", (value, format) => value },
        };

        public string Name { get; }
        public bool IsVariable { get; }
        public string? Type { get; }
        public string? Format { get; }

        public PathSegment(string segment)
        {
            if (segment.StartsWith("{") && segment.EndsWith("}"))
            {
                var parts = segment.Substring(1, segment.Length - 2).Split(':');
                if (parts.Length == 1)
                {
                    Name = parts[0];
                    Type = "string";
                    Format = null;
                }
                else if (parts.Length == 2)
                {
                    Name = parts[0];
                    Type = parts[1];
                    Format = null;
                }
                else if (parts.Length == 2)
                {
                    Name = parts[0];
                    Type = parts[1];
                    Format = parts[2];
                }
                else
                {
                    throw new Exception("Invalid path segment.");
                }

                if (!_converters.ContainsKey(Type))
                    throw new Exception("Invalid type");

                IsVariable = true;
            }
            else if (segment.StartsWith("{") || segment.EndsWith("}"))
                throw new Exception("Invalid substitution segment");
            else if (segment.Contains("{") || segment.Contains("}"))
                throw new Exception("Literal segment contains invalid characters");
            else
            {
                Name = segment;
                IsVariable = false;
                Type = null;
                Format = null;
            }
        }

        public (bool, string?, object?) Match(string value, bool ignoreCase)
        {
            if (!IsVariable)
                return (string.Compare(value, Name, ignoreCase) == 0, null, null);

            try
            {
                var convert = _converters[Type ?? string.Empty];
                var result = convert(value, Format);
                return (true, Name, result);
            }
            catch
            {
                return (false, null, null);
            }
        }

        public override string ToString() => $"Name=\"{Name}\",IsVariable={IsVariable},Type={Type},Format={Format}";
    }
}