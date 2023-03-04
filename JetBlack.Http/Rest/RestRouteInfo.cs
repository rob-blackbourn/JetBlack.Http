using System.Collections.Generic;

namespace JetBlack.Http.Rest
{
    /// <summary>
    /// A class to hold information passed on the invocation of a route.
    /// </summary>
    public class RestRouteInfo
    {
        /// <summary>
        /// Construct the route information.
        /// </summary>
        /// <param name="matches">The route variable matches.</param>
        public RestRouteInfo(Dictionary<string, object?> matches)
        {
            Matches = matches;
        }

        /// <summary>
        /// A dictionary of the variables and values matched by the route.
        /// </summary>
        /// <value>The matched variables and their values.</value>
        public IReadOnlyDictionary<string, object?> Matches { get; }
    }
}