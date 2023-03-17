using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using JetBlack.Http.Rest;

namespace Example
{
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
                var controller = new GreetingController(loggerFactory);

                var server = new RestServer(loggerFactory)
                    .AddPrefix("http://*:8081/")
                    .ConfigureRouter(router => router.IgnoreCase = true)
                    .AddRoute(controller.SayHello, "/helloWorld", "GET")
                    .AddRoute(controller.SayWithQueryString, "/hello")
                    .AddRoute(controller.SayName, "/hello/{name:string}", "GET", "POST")
                    .AddRoute(controller.SayNameAndAge, "/hello/{name:string}/{age:int}");

                await server.RunAsync();
            }
        }
    }
}