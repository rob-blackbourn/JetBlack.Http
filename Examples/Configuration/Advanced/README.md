# Advanced Fluent Configuration

This example uses the `ConfigureLister` and `ConfigureRouter` methods, which
give complete access to the listener and router.

## Usage

With advanced configuration the methods take a function which has as it's
argument the object to be fonfigured.

```csharp
var server = new RestServer(loggerFactory)
    .ConfigureListener(listener => listener.Prefixes.Add("http://*:8081/"))
    .ConfigureRouter(router =>
    {
        router.IgnoreCase = true;

        router.AddRoute(SayHello, "/api/v1/helloWorld", "GET");
        router.AddRoute(SayWithQueryString, "/api/v1/hello"); // GET is the default.
        router.AddRoute(SayName, "/api/v1/hello/{name:string}", "GET", "POST");
        router.AddRoute(SayNameAndAge, "/api/v1/hello/{name:string}/{age:int}");
    });
```

Next: [Manual Configuration](../Manual/)
