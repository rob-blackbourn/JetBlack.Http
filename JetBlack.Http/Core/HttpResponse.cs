using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Text;

namespace JetBlack.Http.Core
{
    /// <summary>
    /// An HTTP response.
    /// </summary>
    public class HttpResponse
    {
        /// <summary>
        /// Construct an HTTP response.
        /// </summary>
        /// 
        /// <param name="statusCode">The status code.</param>
        /// <param name="contentType">The content type - defaults to text/html.</param>
        /// <param name="contentEncoding">An optional content encoding.</param>
        /// <param name="headers">Optional headers.</param>
        /// <param name="body">An optional body.</param>
        public HttpResponse(
            HttpStatusCode statusCode,
            string contentType = "text/html",
            Encoding? contentEncoding = null,
            WebHeaderCollection? headers = null,
            Stream? body = null)
        {
            StatusCode = statusCode;
            ContentType = contentType;
            ContentEncoding = contentEncoding;
            Headers = headers;
            Body = body;
        }

        /// <summary>
        /// The HTTP status code.
        /// </summary>
        /// <value>The status code.</value>
        public HttpStatusCode StatusCode { get; }
        /// <summary>
        /// The content type.
        /// </summary>
        /// <value>The content type.</value>
        public string ContentType { get; }
        /// <summary>
        /// The optional content encoding.
        /// </summary>
        /// <value>The encoding or null.</value>
        public Encoding? ContentEncoding { get; }
        /// <summary>
        /// Optional headers.
        /// </summary>
        /// <value>The headers or null.</value>
        public WebHeaderCollection? Headers { get; }
        /// <summary>
        /// The body.
        /// </summary>
        /// <value>The body bytes or null.</value>
        public Stream? Body { get; }

        internal async Task Apply(HttpListenerResponse response)
        {
            response.StatusCode = (int)StatusCode;
            response.ContentType = ContentType;
            response.ContentEncoding = ContentEncoding;

            if (Headers != null)
            {
                foreach (var key in Headers.AllKeys)
                    response.AddHeader(key, Headers[key]);
            }

            if (Body != null)
            {
                await Body.CopyToAsync(response.OutputStream);
            }

            response.Close();
        }

        /// <summary>
        /// A helper method to create a response from a string.
        /// </summary>
        /// <param name="text">The string text.</param>
        /// <param name="statusCode">The optional status code - default to OK.</param>
        /// <param name="contentType">The optional content type - defaults to text/plain.</param>
        /// <param name="contentEncoding">The optional content encoding.</param>
        /// <param name="headers">The optional headers.</param>
        /// <returns>An HTTP response.</returns>
        public static HttpResponse FromString(
            string text,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string contentType = "text/plain",
            Encoding? contentEncoding = null,
            WebHeaderCollection? headers = null
            )
        {
            return FromBytes(
                Encoding.UTF8.GetBytes(text),
                statusCode,
                contentType,
                contentEncoding ??= Encoding.UTF8,
                headers
            );
        }

        /// <summary>
        /// A helper method to create an HTTP response from bytes.
        /// </summary>
        /// <param name="body">The bytes of the body.</param>
        /// <param name="statusCode">An optional status code - defaults to OK.</param>
        /// <param name="contentType">An optional content type - defaults to application/octet-stream.</param>
        /// <param name="contentEncoding">An optional content encoding.</param>
        /// <param name="headers">Optional headers.</param>
        /// <returns>An HTTP response.</returns>
        public static HttpResponse FromBytes(
            byte[] body,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            string contentType = "application/octet-stream",
            Encoding? contentEncoding = null,
            WebHeaderCollection? headers = null
            )
        {
            if (headers == null)
                headers = new WebHeaderCollection();

            headers["Content-Length"] = body.LongLength.ToString();

            return new HttpResponse(
                statusCode,
                contentType,
                contentEncoding,
                headers,
                new MemoryStream(body));
        }
    }
}