using System.Net;
using System.Threading.Tasks;

using JetBlack.HttpServer;

namespace Example
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var controller = new HelloWorldController();

            var server = new HttpServer(() =>
            {
                var listener = new HttpListener();
                listener.Prefixes.Add("http://localhost:8081/");

                return listener;
            })
            .AddRoute("/api/v1/helloWorld", controller.SayHello);

            await server.RunAsync();
        }
    }
}