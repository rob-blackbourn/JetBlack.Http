# Controllers

This example demonstrates how to use a controller class.

The previous program demonstrated binding a local function to a route. A popular
way of providing handlers is to bundle them with a class. This can be useful
when the class has state. For example it may maintain an inventory.

## A Controller Class

The class is like any C# class. Here is part of the class in this example.

```csharp
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using JetBlack.Http.Core;
using JetBlack.Http.Rest;

namespace Example
{
    using RestRequest = HttpRequest<RestRouteInfo, RestServerInfo>;

    internal class GreetingController
    {
        private readonly ILogger _logger;

        public GreetingController(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<GreetingController>();
        }

        public Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
        {
            _logger.LogTrace("In SayHello");

            var response = HttpResponse.FromString("Hello, World!");

            return Task.FromResult(response);
        }

        ...
    }
}
```

The constructor takes a logger factory. The request handlers are taken from a
previous example, and add some logging.

The handlers are added to routes in the same way as local methods.

```csharp
    var controller = new GreetingController(loggerFactory);

    var server = new RestServer(loggerFactory)
        .AddPrefix("http://*:8081/")
        .ConfigureRouter(router => router.IgnoreCase = true)
        .AddRoute(controller.SayHello, "/api/v1/helloWorld", "GET")
        ...
```

Note how the controller is created before the routes are registered.

Next: [Controllers With Attributes](../ControllerWithAttributes/)
