using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using JetBlack.Http.Core;

namespace JetBlack.Http.Middleware
{
    public class CompressionMiddleware<TRouteInfo, TServerInfo>
        where TRouteInfo : class
        where TServerInfo : class
    {
        private static Func<Stream, Stream>? GetCompressorFactory(string? compression)
        {
            switch (compression)
            {
                case "gzip":
                    return stream => new GZipStream(stream, CompressionMode.Compress);
                case "deflate":
                    return stream => new DeflateStream(stream, CompressionMode.Compress);
                default:
                    return null;
            }
        }

        private async Task<(Stream?, string?)> GetCompressedStream(string? acceptEncoding, Stream? uncompressedStream)
        {
            if (acceptEncoding == null || uncompressedStream == null)
                return (uncompressedStream, null);

            foreach (var encoding in acceptEncoding.Split(',').Select(x => x.Trim()))
            {
                var parts = encoding.Split(';');
                if (parts.Length == 0)
                    continue;
                var compression = parts[0];

                var compressorFactory = GetCompressorFactory(compression);
                if (compressorFactory == null)
                    continue;

                using (var buffer = new MemoryStream())
                {
                    using (var compressor = compressorFactory(buffer))
                    {
                        await uncompressedStream.CopyToAsync(compressor);
                        compressor.Close();
                    }
                    var compressedStream = new MemoryStream(buffer.ToArray());
                    return (compressedStream, compression);
                }
            }

            return (uncompressedStream, "identity");
        }

        /// <summary>
        /// Apply the middleware to a request.
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="handler">The handler to call.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>The response, compressed where requested.</returns>
        public async Task<HttpResponse> Apply(
            HttpRequest<TRouteInfo, TServerInfo> request,
            Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>> handler,
            CancellationToken token)
        {
            var response = await handler(request, token);

            if (((int)response.StatusCode) < 200 || ((int)response.StatusCode) >= 300)
                return response;

            var requestHeaders = request.Context.Request.Headers;
            var acceptEncoding = requestHeaders["Accept-Encoding"];

            var responseHeaders = response.Headers ?? new WebHeaderCollection();
            var (body, contentEncoding) = await GetCompressedStream(acceptEncoding, response.Body);

            if (contentEncoding != null && body != null)
            {
                responseHeaders["Content-Length"] = body.Length.ToString();
                responseHeaders["Content-Encoding"] = contentEncoding;
            }

            return new HttpResponse(
                response.StatusCode,
                response.ContentType,
                response.ContentEncoding,
                responseHeaders,
                body);
        }

        /// <summary>
        /// Create the middleware handler.
        /// </summary>
        /// <returns>A middleware handler.</returns>
        public static Func<HttpRequest<TRouteInfo, TServerInfo>, Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>>, CancellationToken, Task<HttpResponse>> Create()
        {
            var middleware = new CompressionMiddleware<TRouteInfo, TServerInfo>();
            return middleware.Apply;
        }
    }
}
