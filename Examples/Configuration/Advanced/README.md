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

        router.AddRoute(SayHello, "/helloWorld", "GET");
        router.AddRoute(SayWithQueryString, "/hello"); // GET is the default.
        router.AddRoute(SayName, "/hello/{name:string}", "GET", "POST");
        router.AddRoute(SayNameAndAge, "/hello/{name:string}/{age:int}");
    });
```

Note the `router.IgnoreCase = true;` line. This configuration was not possible
with the simple methods. However it is possible to use both. For example

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
