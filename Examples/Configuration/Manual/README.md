# Manual Configuration

This example builds the server from scratch using none of the fluent helper
methods.

Four endpoints are defined:

* `http://localhost:8081/helloWorld`, `GET` - the server response with `"Hello, World!"`.
    Note the router is configured to case insensitive.
* `http://localhost:8081/hello` - Takes the parameters as a query string,
    and demonstrates that `GET` is the default method. e.g. `http://localhost:8081/hello?name=mary&age=12`
* `http://localhost:8081/hello/{name:string}`, `GET` and `POST` - demonstrates
    a path with a variable and specifying multiple methods. An example might be: 
    `http://localhost:8081/hello/mary`.
* `http://localhost:8081/hello/{name:string}/{age:int}` - demonstrates
    multiple parameters: e.g. `http://localhost:8081/hello/mary/12`.

## Usage

Here is the configuration from the code:

```csharp
// Setup the listener.
var listener = new HttpListener();
listener.Prefixes.Add("http://*:8081/");

// Setup the router.
var router = new RestRouter(true, loggerFactory);
router.AddRoute(SayHello, "/helloWorld", "GET");
router.AddRoute(SayWithQueryString, "/hello"); // GET is the default.
router.AddRoute(SayName, "/hello/{name:string}", "GET", "POST");
router.AddRoute(SayNameAndAge, "/hello/{name:string}/{age:int}");

// Make a list of middlewares.
var middlewares = new List<
    Func<
        HttpRequest<RestRouteInfo, RestServerInfo>,
        Func<HttpRequest<RestRouteInfo, RestServerInfo>, CancellationToken, Task<HttpResponse>>,
        CancellationToken,
        Task<HttpResponse>>>();

// Make a list of startup and shutdown handlers.
var startupHandlers = new List<Func<RestServerInfo, CancellationToken, Task>>();
var shutdownHandlers = new List<Func<RestServerInfo, CancellationToken, Task>>();

// Make the server.
var server = new RestServer(
    listener,
    router,
    middlewares,
    startupHandlers,
    shutdownHandlers,
    loggerFactory);
```

Next: [Routes and Request Handlers](../../RoutesAndRequestHandlers/) or [up](..).
