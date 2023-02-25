using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace JetBlack.HttpServer
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; }
        public WebHeaderCollection? Headers { get; }
        public byte[]? Body { get; }

        public HttpResponse(HttpStatusCode statusCode, WebHeaderCollection? headers = null, byte[]? body = null)
        {
            StatusCode = statusCode;
            Headers = headers;
            Body = body;
        }

        public async Task Apply(HttpListenerResponse response)
        {
            response.StatusCode = (int) StatusCode;

            if (Headers != null)
            {
                foreach (var key in Headers.AllKeys)
                    response.Headers[key] = Headers[key];
            }

            if (Body != null)
                await response.OutputStream.WriteAsync(Body, 0, Body.Length);

            response.Close();
        }

        public static HttpResponse FromString(string text, WebHeaderCollection? headers = null, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var body = Encoding.UTF8.GetBytes(text);
            if (headers == null)
                headers = new WebHeaderCollection();
            if (headers["Content-Type"] == null)
                headers["Content-Type"] = "text/plain";

            return new HttpResponse(statusCode, headers, body);
        }
    }
}