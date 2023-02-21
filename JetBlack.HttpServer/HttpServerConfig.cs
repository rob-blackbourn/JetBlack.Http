using System.Collections.Generic;

namespace HttpServer
{
    public class HttpServerConfig
    {
        public List<string> ListenerPrefixes { get; set; } = new List<string>();

        private HttpServerConfig()
        {
        }
    }
}
