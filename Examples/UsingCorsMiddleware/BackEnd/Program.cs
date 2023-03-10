using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using JetBlack.Http.Core;
using JetBlack.Http.Middleware;
using JetBlack.Http.Rest;

namespace Example
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;
    using RestCorsMiddleware = CorsMiddleware<RestRouteInfo, RestServerInfo>;

    internal class Program
    {
        private static readonly object _gate = new object();
        static async Task Main(string[] args)
        {
            using (var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            }))
            {
                var server = new RestServer(loggerFactory)
                    .AddPrefix("http://*:9010/")
                    .ConfigureRouter(router => router.IgnoreCase = true)
                    .AddMiddleware(RestCorsMiddleware.Create(loggerFactory: loggerFactory))
                    .AddRoute(GetInfo, "/info", "GET")
                    .AddRoute(SetInfo, "/info", "POST", "OPTIONS")
                    .ConfigureServerInfo(info =>
                    {
                        info.Data["name"] = "Michael Caine";
                    });

                await server.RunAsync();
            }
        }

        public static Task<HttpResponse> GetInfo(RestRequest request, CancellationToken token)
        {
            lock (_gate)
            {
                var text = JsonConvert.SerializeObject(request.ServerInfo.Data);
                var response = HttpResponse.FromString(
                    text,
                    statusCode: HttpStatusCode.OK,
                    contentType: "application/json");

                return Task.FromResult(response);
            }
        }

        public static async Task<HttpResponse> SetInfo(RestRequest request, CancellationToken token)
        {
            var buffer = new byte[2048];
            var bytesRead = await request.Body.ReadAsync(buffer, 0, buffer.Length);
            var text = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(text);
            if (data != null && data.ContainsKey("name"))
            {
                lock (_gate)
                {
                    request.ServerInfo.Data["name"] = data["name"];
                }
            }

            var response = HttpResponse.FromString(
                "Hello, World!",
                statusCode: HttpStatusCode.OK);

            return response;
        }
    }
}