using System.Collections.Generic;

namespace JetBlack.HttpServer
{
    public class HttpServerConfig
    {
        public List<string> ListenerPrefixes { get; set; } = new List<string>();

        private HttpServerConfig()
        {
        }
    }
}
