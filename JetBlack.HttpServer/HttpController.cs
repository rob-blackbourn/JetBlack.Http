using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpServer
{
    public abstract class HttpController
    {
        private readonly Dictionary<HttpMethod, Func<HttpRequest, HttpResponse, Task>> _httpMethodToDelegateMap;

        protected HttpController()
        {
            _httpMethodToDelegateMap = new Dictionary<HttpMethod, Func<HttpRequest, HttpResponse, Task>>
            {
                { HttpMethod.Post, PostAsync },
                { HttpMethod.Put, PutAsync },
                { HttpMethod.Get, GetAsync },
                { HttpMethod.Delete, DeleteAsync },
                { HttpMethod.Head, HeadAsync },
                // { HttpMethod.Patch, PatchAsync },
                { HttpMethod.Options, OptionsAsync },
                { HttpMethod.Trace, TraceAsync }
            };
        }

        public virtual Task PostAsync(HttpRequest req, HttpResponse res) =>
            SendDefaultResponse(res);

        public virtual Task PutAsync(HttpRequest req, HttpResponse res) =>
            SendDefaultResponse(res);

        public virtual Task GetAsync(HttpRequest req, HttpResponse res) =>
            SendDefaultResponse(res);

        public virtual Task DeleteAsync(HttpRequest req, HttpResponse res) =>
            SendDefaultResponse(res);

        public virtual Task HeadAsync(HttpRequest req, HttpResponse res) =>
            SendDefaultResponse(res);

        public virtual Task PatchAsync(HttpRequest req, HttpResponse res) =>
            SendDefaultResponse(res);

        public virtual Task OptionsAsync(HttpRequest req, HttpResponse res) =>
            SendDefaultResponse(res);

        public virtual Task TraceAsync(HttpRequest req, HttpResponse res) =>
            SendDefaultResponse(res);

        internal virtual Task HandleAnyAsync(HttpRequest req, HttpResponse res)
        {
            try
            {
                var httpMethod = new HttpMethod(req.Method);

                return _httpMethodToDelegateMap[httpMethod]
                    .Invoke(req, res);
            }
            catch
            {
                return SendDefaultResponse(res);
            }
        }

        protected static Task SendDefaultResponse(HttpResponse res)
        {
            return res.AnswerWithStatusCodeAsync(
                HttpStatusCode.NotImplemented);
        }
    }
}
