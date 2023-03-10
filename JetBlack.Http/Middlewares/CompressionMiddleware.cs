using System;
using System.Collections.Specialized;
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
        private async Task<(Stream?, string?)> GetCompressedStream(string? acceptEncoding, Stream? uncompressedStream)
        {
            if (acceptEncoding == null || uncompressedStream == null)
                return (uncompressedStream, null);

            foreach (var encoding in acceptEncoding.Split(',').Select(x => x.Trim()))
            {
                var parts = encoding.Split(';');
                if (parts.Length == 0)
                    continue;

                switch (parts[0])
                {
                    case "gzip":
                        {
                            var buffer = new MemoryStream();
                            var compressor = new GZipStream(buffer, CompressionMode.Compress);
                            await uncompressedStream.CopyToAsync(compressor);
                            compressor.Close();
                            var compressedStream = new MemoryStream(buffer.ToArray());
                            return (compressedStream, "gzip");
                        }
                    case "deflate":
                        {
                            var buffer = new MemoryStream();
                            var compressor = new DeflateStream(buffer, CompressionMode.Compress);
                            await uncompressedStream.CopyToAsync(compressor);
                            compressor.Close();
                            var compressedStream = new MemoryStream(buffer.ToArray());
                            return (compressedStream, "gzip");
                        }
                }
            }

            return (uncompressedStream, "identity");
        }

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
                responseHeaders.Remove("Content-Length");
                // responseHeaders["Content-Length"] = body.Length.ToString();
                responseHeaders["Content-Encoding"] = contentEncoding;
            }

            return new HttpResponse(
                response.StatusCode,
                response.ContentType,
                response.ContentEncoding,
                responseHeaders,
                body);
        }

        public static Func<HttpRequest<TRouteInfo, TServerInfo>, Func<HttpRequest<TRouteInfo, TServerInfo>, CancellationToken, Task<HttpResponse>>, CancellationToken, Task<HttpResponse>> Create()
        {
            var middleware = new CompressionMiddleware<TRouteInfo, TServerInfo>();
            return middleware.Apply;
        }
    }
}
