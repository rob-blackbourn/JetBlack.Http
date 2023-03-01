using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JetBlack.Http
{
    public interface IHttpRouter
    {
        bool IgnoreCase { get; set; }
        void AddRoute(string path, Func<HttpRequest, Task<HttpResponse>> handler);
        (Func<HttpRequest, Task<HttpResponse>>, Dictionary<string,object?>?) FindHandler(string path);
    }
}
