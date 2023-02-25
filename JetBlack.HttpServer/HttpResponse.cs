using System.Net;

namespace JetBlack.HttpServer
{
    public class HttpResponse
    {
        public HttpListenerContext Context { get; }
        public HttpListenerResponse Response => Context.Response;

        public HttpResponse(HttpListenerContext context)
        {
            Context = context;
        }
    }
}