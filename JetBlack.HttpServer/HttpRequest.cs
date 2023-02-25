using System.Collections.Generic;
using System.Net;

namespace JetBlack.HttpServer
{
    public class HttpRequest
    {
        public HttpListenerContext Context { get; }
        public HttpListenerRequest Request => Context.Request;
        public Dictionary<string, object?>? Matches { get; set; } = null;

        public HttpRequest(HttpListenerContext context)
        {
            Context = context;
        }
    }
}