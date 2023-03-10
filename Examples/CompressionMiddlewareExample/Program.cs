﻿using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using JetBlack.Http.Middleware;
using JetBlack.Http.Rest;

namespace Example
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;
    using RestCompressionMiddleware = CompressionMiddleware<RestRouteInfo, RestServerInfo>;

    internal class Program
    {
        static async Task Main(string[] args)
        {
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