# Simple Fluent Configuration

This example demonstrates how to configure the server using fluent
configuration.

Four endpoints are defined:

* `http://localhost:8081/api/v1/helloWorld`, `GET` - the server response with `"Hello, World!"`.
* `http://localhost:8081/api/v1/hello` - Takes the parameters as a query string,
    and demonstrates that `GET` is the default method. e.g. `http://localhost:8081/api/v1/hello?name=mary&age=12`
* `http://localhost:8081/api/v1/hello/{name:string}`, `GET` and `POST` - demonstrates
    a path with a variable and specifying multiple methods. An example might be: 
    `http://localhost:8081/api/v1/hello/mary`.
* `http://localhost:8081/api/v1/hello/{name:string}/{age:int}` - demonstrates
    multiple parameters: e.g. `http://localhost:8081/api/v1/hello/mary/12`.

## Usage

Here is the configuration for the server:

```csharp
var server = new RestServer(loggerFactory)
    .AddPrefix("http://*:8081/")
    .AddRoute(SayHello, "/api/v1/helloWorld", "GET")
    .AddRoute(SayWithQueryString, "/api/v1/hello")
    .AddRoute(SayName, "/api/v1/hello/{name:string}", "GET", "POST")
    .AddRoute(SayNameAndAge, "/api/v1/hello/{name:string}/{age:int}");
```

### `AddPrefix`

The `AddPrefix` method takes a base endpoint for the server to bind to.

### `AddRoute`

The ``AddRoute` method binds a request handler method to a path and HTTP method
or methods.

### `AddMiddleware`

The `AddMiddleware` method adds a middleware handler. The middleware handles
are called in the order in which they were added.

Next: [Advanced Fluent Configuration](../Advanced/) or [up](..).
