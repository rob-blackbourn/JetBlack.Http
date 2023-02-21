using System.Net;

namespace JetBlack.HttpServer
{
    public class HttpResponse
    {
        public HttpListenerResponse Response { get; }

        public HttpResponse(HttpListenerResponse res)
        {
            Response = res;
        }
    }
}