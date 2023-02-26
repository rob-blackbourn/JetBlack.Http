using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace JetBlack.Http
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; }
        public string ContentType { get;}
        public Encoding? ContentEncoding { get; }
        public WebHeaderCollection? Headers { get; }
        public byte[]? Body { get; }

        public HttpResponse(
            HttpStatusCode statusCode,
            string contentType = "text/html",
            Encoding? contentEncoding = null,
            WebHeaderCollection? headers = null,
            byte[]? body = null)
        {
            StatusCode = statusCode;
            ContentType = contentType;
            ContentEncoding = contentEncoding;
            Headers = headers;
            Body = body;
        }

        public async Task Apply(HttpListenerResponse response)
        {
            response.StatusCode = (int) StatusCode;
            response.ContentType = ContentType;
            response.ContentEncoding = ContentEncoding;

            if (Headers != null)
            {
                foreach (var key in Headers.AllKeys)
                    response.AddHeader(key, Headers[key]);
            }

            if (Body != null)
            {
                response.ContentLength64 = Body.LongLength;
                await response.OutputStream.WriteAsync(Body, 0, Body.Length);
            }

            response.Close();
        }

        public static HttpResponse FromString(
            string text,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string contentType = "text/plain",
            Encoding? contentEncoding = null,
            WebHeaderCollection? headers = null
            )
        {
            contentEncoding ??= Encoding.UTF8;
            var body = Encoding.UTF8.GetBytes(text);

            return new HttpResponse(
                statusCode,
                contentType,
                contentEncoding,
                headers,
                body);
        }

        public static HttpResponse FromBytes(
            byte[] body,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string contentType = "application/octet-stream",
            Encoding? contentEncoding = null,
            WebHeaderCollection? headers = null
            )
        {
            return new HttpResponse(
                statusCode,
                contentType,
                contentEncoding,
                headers,
                body);
        }
    }
}