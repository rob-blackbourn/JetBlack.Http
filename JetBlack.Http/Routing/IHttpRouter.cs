using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JetBlack.Http.Routing
{
    public interface IHttpRouter
    {
        (Func<HttpRequest, Task<HttpResponse>>, Dictionary<string,object?>?) FindHandler(string path);

        void AddRoute(
            string path,
            Func<HttpRequest, Task<HttpResponse>> handler);
    }
}
