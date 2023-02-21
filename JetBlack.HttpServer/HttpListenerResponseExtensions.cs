using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Text;

namespace HttpServer
{
    public static class HttpListenerResponseExtensions
    {
        /// <summary>
        /// Answers a request with the given status code and a response body.
        /// </summary>
        /// <param name="body">The response body as a string. UTF-8 encoding is used. If another encoding is needed the <seealso cref="AnswerWithStatusCodeAsync(HttpListenerResponse, byte[], HttpStatusCode)"/> method should be used.</param>
        /// <param name="statusCode">The status code which should be send back to the client.</param>
        public static async Task AnswerWithStatusCodeAsync(
            this HttpResponse res,
            string body,
            HttpStatusCode statusCode)
        {
            var bytes = Encoding.UTF8.GetBytes(body);

            res.Response.ContentEncoding = Encoding.UTF8;

            await AnswerWithStatusCodeAsync(
                res,
                bytes,
                statusCode,
                MediaTypeNames.Text.Plain);
        }

        /// <summary>
        /// Answers a request with the given status code and no response body.
        /// </summary>
        /// <param name="res"></param>
        /// <param name="statusCode"></param>
        public static Task AnswerWithStatusCodeAsync(
            this HttpResponse res,
            HttpStatusCode statusCode)
        {
            res.Response.StatusCode = (int)statusCode;
            res.Response.Close();

            return Task.CompletedTask;
        }

        /// <summary>
        /// Answers a request with the given status code and a response body.
        /// </summary>
        /// <param name="statusCode">The status code which should be send back to the client.</param>
        public static async Task AnswerWithStatusCodeAsync(
            this HttpResponse res, 
            byte[] body, 
            HttpStatusCode statusCode,
            string contentType = MediaTypeNames.Application.Octet)
        {
            res.Response.ContentType ??= contentType;
            res.Response.ContentLength64 = body.Length;
            res.Response.StatusCode = (int)statusCode;

            await res.Response.OutputStream.WriteAsync(body, 0, body.Length);
            res.Response.Close();
        }
    }
}
