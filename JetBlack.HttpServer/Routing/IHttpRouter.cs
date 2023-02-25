using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JetBlack.HttpServer.Routing
{
    public interface IHttpRouter
    {
        (Func<HttpRequest, HttpResponse, Task>, Dictionary<string,object?>?) FindHandler(string path);

        void AddRoute(
            string path,
            Func<HttpRequest, HttpResponse, Task> handler);
    }
}
