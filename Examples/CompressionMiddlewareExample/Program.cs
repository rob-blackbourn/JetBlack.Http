using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using JetBlack.Http.Middleware;
using JetBlack.Http.Rest;
using System.IO;
using System.IO.Compression;

namespace Example
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;
    using RestCompressionMiddleware = CompressionMiddleware<RestRouteInfo, RestServerInfo>;

    internal class Program
    {
        public static byte[] Compress(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
                {
                    gzipStream.Write(bytes, 0, bytes.Length);
                }
                return memoryStream.ToArray();
            }
        }

        public static byte[] Decompress(byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {

                using (var outputStream = new MemoryStream())
                {
                    using (var decompressStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                    {
                        decompressStream.CopyTo(outputStream);
                    }
                    return outputStream.ToArray();
                }
            }
        }
        public static MemoryStream Compress2(Stream uncompressed)
        {
            var memoryStream = new MemoryStream();
            using (var gzipStream = new GZipStream(memoryStream, CompressionLevel.Optimal))
            {
                uncompressed.CopyTo(gzipStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
            }
            return memoryStream;
        }

        public static MemoryStream Decompress2(Stream compressedStream)
        {
            var outputStream = new MemoryStream();

            using (var decompressStream = new GZipStream(compressedStream, CompressionMode.Decompress))
            {
                decompressStream.CopyTo(outputStream);
            }
            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }

        static async Task Main(string[] args)
        {
            // var text = "This is not a test";
            // var buf = Encoding.UTF8.GetBytes(text);
            // var compressed = Compress(buf);
            // var decompressed = Decompress(compressed);
            // var result = Encoding.UTF8.GetString(decompressed);

            // var uncompressedStream = new MemoryStream(buf);
            // var compressedStream = Compress2(uncompressedStream);
            // var decompressedStream = Decompress2(compressedStream);
            // var outBuf = decompressedStream.ToArray();
            // var result2 = Encoding.UTF8.GetString(outBuf);


            using (var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            }))
            {
                var server = new RestServer(loggerFactory)
                    .AddPrefix("http://*:8081/")
                    .ConfigureRouter(router => router.IgnoreCase = true)
                    .AddMiddleware(RestCompressionMiddleware.Create())
                    .AddRoute(SayHello, "/api/v1/helloWorld", "GET");

                await server.RunAsync();
            }
        }

        public static Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
        {
            var text =
                "I know I have the body of a weak and feeble woman; but I have " +
                "the heart and stomach of a king, and of a king of England too, " +
                "and think foul scorn that Parma or Spain, or any prince of " +
                "Europe, should dare to invade the borders of my realm: to which " +
                "rather than any dishonour shall grow by me, I myself will take " +
                "up arms, I myself will be your general, judge, and rewarder of " +
                "every one of your virtues in the field.";

            var response = HttpResponse.FromString(text, statusCode: HttpStatusCode.OK);

            return Task.FromResult(response);
        }
    }
}