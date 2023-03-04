using System.Collections.Generic;

namespace JetBlack.Http.Rest
{
    public class RestRouteInfo
    {
        public Dictionary<string, object?> Matches { get; }

        public RestRouteInfo(Dictionary<string, object?> matches)
        {
            Matches = matches;
        }
    }
}