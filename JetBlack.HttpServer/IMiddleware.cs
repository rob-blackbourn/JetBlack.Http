using System.Threading.Tasks;

namespace HttpServer
{
    public interface IMiddleware
    {
        Task HandleRequestAsync(
            HttpRequest req,
            HttpResponse res);
    }
}
