using System.Net;

namespace JetBlack.HttpServer
{
    public class HttpRequest
    {
        public HttpListenerContext Context { get; }
        public HttpListenerRequest Request => Context.Request;


        public HttpRequest(HttpListenerContext context)
        {
            Context = context;
        }
    }
}