using System.Threading.Tasks;

namespace JetBlack.HttpServer
{
    public interface IMiddleware
    {
        Task HandleRequestAsync(
            HttpRequest req,
            HttpResponse res);
    }
}
