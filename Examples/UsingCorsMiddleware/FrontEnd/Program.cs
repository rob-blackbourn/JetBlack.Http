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
    using RestCorsMiddleware = CorsMiddleware<RestRouteInfo, RestServerInfo>;

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
                    .AddPrefix("http://*:9009/")
                    .ConfigureRouter(router => router.IgnoreCase = true)
                    .AddMiddleware(RestCorsMiddleware.Create(loggerFactory: loggerFactory))
                    .AddRoute(RenderPage, "/index.html", "GET");

                await server.RunAsync();
            }
        }

        public static Task<HttpResponse> RenderPage(RestRequest request, CancellationToken token)
        {
            var hostname = Dns.GetHostName();

            var page = @"
<!DOCTYPE html>
<html>
  <head>
    <meta charset='utf-8'>
    <meta name=""viewport"" content='width=device-width, initial-scale=1.0'>
    <title>Web Server</title>
  </head>
  <body>
    <form>
        Info: <input name='info' type='text' id='info'><br />
        <button type='button' onclick='postInfo()'>Post</button>
    </form>
    
    <script>
      function postInfo() {
        const element = document.getElementById('info');
        const data = { name: element.value };

        fetch('http://%HOSTNAME%:9010/info', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json'
          },
          body: JSON.stringify(data)
        })
          .then(function(response) {
            console.log(response);
            return Promise.resolve('Done');
          });
      }
      
      window.onload = function() {fetch('http://%HOSTNAME%:9010/info')
          .then(function(response) {
            return response.json();
          })
          .then(function(info) {
            const element = document.getElementById('info');
            element.value = info.name;
          });
      }
    </script>
  </body>
</html>    
".Replace("%HOSTNAME%", "localhost");

            var response = HttpResponse.FromString(page, 200, "text/html");

            return Task.FromResult(response);
        }
    }
}