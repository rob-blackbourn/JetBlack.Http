# Simple Fluent Configuration

This example demonstrates how to configure the server using fluent
configuration.

Four endpoints are defined:

* `http://localhost:8081/helloworld`, `GET` - the server response with `"Hello, World!"`.
* `http://localhost:8081/hello` - Takes the parameters as a query string,
    and demonstrates that `GET` is the default method. e.g. `http://localhost:8081/hello?name=mary&age=12`
* `http://localhost:8081/hello/{name:string}`, `GET` and `POST` - demonstrates
    a path with a variable and specifying multiple methods. An example might be: 
    `http://localhost:8081/hello/mary`.
* `http://localhost:8081/hello/{name:string}/{age:int}` - demonstrates
    multiple parameters: e.g. `http://localhost:8081/hello/mary/12`.

## Usage

Here is the configuration for the server:

```csharp
var server = new RestServer(loggerFactory)
    .AddPrefix("http://*:8081/")
    .AddRoute(SayHello, "/helloWorld", "GET")
    .AddRoute(SayWithQueryString, "/hello")
    .AddRoute(SayName, "/hello/{name:string}", "GET", "POST")
    .AddRoute(SayNameAndAge, "/hello/{name:string}/{age:int}");
```

### `AddPrefix`

The `AddPrefix` method takes a base endpoint for the server to bind to.

### `AddRoute`

The ``AddRoute` method binds a request handler method to a path and HTTP method
or methods.

### `AddMiddleware`

The `AddMiddleware` method adds a middleware handler. The middleware handles
are called in the order in which they were added.

### `AddStartupHandler`

The `AddStartupHandler` adds a startup handler. The handlers are called in the
order the are added.

### `AddShutdownHandler`

The `AddShutdownHandler` adds a startup handler. The handlers are called in the
order the are added.


Next: [Advanced Fluent Configuration](../Advanced/) or [up](..).
