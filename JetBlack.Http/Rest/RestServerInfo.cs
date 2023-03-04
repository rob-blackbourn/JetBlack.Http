using System.Collections.Generic;

namespace JetBlack.Http.Rest
{
    /// <summary>
    /// The server information which exists for the lifetime of the server.
    /// </summary>
    public class RestServerInfo
    {
        /// <summary>
        /// A dictionary to store persistent information.
        /// </summary>
        /// <typeparam name="string">The key.</typeparam>
        /// <typeparam name="object">The value</typeparam>
        /// <returns>A key value store.</returns>
        public IDictionary<string, object> Data { get; } = new Dictionary<string, object>();
    }
}