using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JetBlack.Http
{
    public interface IHttpRouter
    {
        void AddRoute(string path, Func<HttpRequest, Task<HttpResponse>> handler);
        (Func<HttpRequest, Task<HttpResponse>>, Dictionary<string, object?>?) FindHandler(string path);
    }
}
