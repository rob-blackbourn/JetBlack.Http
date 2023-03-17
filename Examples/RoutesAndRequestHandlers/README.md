# Routes and Request Handlers

An HTTP server needs to bind *routes* (the path that is entered into the browser)
with *request handlers* (code that is run to provide a response to the request).

## Routes

A *route path* is the part of the path entered in the browser that follows the prefix
set while configuring the server. For example: if the server is listening on the
prefix `http://localhost:8081/` and the full path is `http://localhost:8081/book/42`,
the *route path* would be `/book/42`.

The route path is associated with an *HTTP method*. For example a `GET` on the
route path `/book/42` might return the details for a book with the id 42,
while a `POST` to the same path might update the details.

So a route is a combination of the path and the HTTP method.

### Path Variables

Routes may contain *path variables* capture *segments* of the path. A segment is
the part between two "/" or the final "/" and the end of the path. For example
the path `/book/42` has two segments; `book` and `42`. In this case the `book`
is a constant (perhaps indicating the domain of things the route can handle),
and `42` is a variable (perhaps identifying the id of the book).

These types of paths can be matched by using path variables. To match the book
path we might use `"/book/{bookId:int}"`. The `bookId` will be parsed as an `int`
and passed through in the request.

There are several types that are supported:

* `string`
* `int`
* `double`
* `datetime`
* `path`

The `path` is a special type which matches the remainder of the path. As such it
must be the last segment of the path. For example if the route path
`/book/{rest:path}` was given `/book/foo/bar/grum` the `rest` variable would
contain `foo/bar/grum`.

The path variables are stored in the `RouteInfo.Matches` property of the request
which is an `IReadonlyDictionary<string, object?>` with the variable name as the
key.

## Request Handlers

A request handler is bound to a route path and method, and is called when a request
is received that matches the route. The request is of type `HttpRequest`. 

The request handler returns an `HttpResponse` object.

### HttpRequest

The request has four properties:

#### HttpRequest.Context

The `Context` is an [`HttpListenerContext`](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistenercontext)
object. It contains all the information the listener provides. This includes the
requested path, query string, etc.

#### HttpRequest.RouteInfo

For the `RestServer` this is a `RestRouteInfo` object which has a
single property `Matches` which is an `IReadonlyDictionary<string, object?>`
object containing any path variables that were matched.

#### HttpRequest.ServerInfo

For the `RestServer` a `RestServerInfo` object which contains a
single `Data` property of type `IDictionary<string, object>`.

### HttpResponse

The response constructor contains four arguments:

* statusCode - An [`HttpStatusCode`](https://learn.microsoft.com/en-us/dotnet/api/system.net.httpstatuscode) statusCode.
* contentType - The `Content-Type` header value.
            string contentType = "text/html",
            Encoding? contentEncoding = null,
            WebHeaderCollection? headers = null,
            Stream? body = null)

Next: [Overview](./Overview/) or [up](..).
