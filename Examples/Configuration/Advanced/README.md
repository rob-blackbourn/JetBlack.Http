# Advanced Fluent Configuration

This example uses the `ConfigureLister` and `ConfigureRouter` methods, which
give complete access to the listener and router.

## Usage

With advanced configuration the methods take a function which has as it's
argument the object to be configured.

The advanced configuration methods are:

* `ConfigureListener` - passes [`HttpListener`](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener).
* `ConfigureRouter` - passes `RestRouter`.
* `ConfigureMiddleware` - passes the list of middleware.
* `ConfigureStartupHandler` - passes the list of startup handlers.
* `ConfigureShutdownHandler` - passes the list of shutdown handlers.
* `ConfigureServerInfo` - passes `RestServerInfo`.

Here is the configuration code from the example program.

```csharp
var server = new RestServer(loggerFactory)
    .ConfigureListener(listener => listener.Prefixes.Add("http://*:8081/"))
    .ConfigureRouter(router =>
    {
        router.IgnoreCase = true;

        router.AddRoute(SayHello, "/helloWorld", "GET");
        router.AddRoute(SayWithQueryString, "/hello"); // GET is the default.
        router.AddRoute(SayName, "/hello/{name:string}", "GET", "POST");
        router.AddRoute(SayNameAndAge, "/hello/{name:string}/{age:int}");
    });
```

Note the `router.IgnoreCase = true;` line. This configuration was not possible
with the simple methods. However it is possible to use both styles. For example:

```csharp
var server = new RestServer(loggerFactory)
    .AddPrefix("http://*:8081/")
    .ConfigureRouter(router => router.IgnoreCase = true)
    .AddRoute(SayHello, "/helloworld", "GET")
    .AddRoute(SayWithQueryString, "/hello")
    .AddRoute(SayName, "/hello/{name:string}", "GET", "POST")
    .AddRoute(SayNameAndAge, "/hello/{name:string}/{age:int}");
```

Next: [Manual Configuration](../Manual/) or [up](..).
