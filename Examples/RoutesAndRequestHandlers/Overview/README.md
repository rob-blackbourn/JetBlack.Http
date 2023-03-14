# An Overview of Routes and Request Handlers

This program gives an overview of routes, and request handlers.

## Case Insensitive Routing

The rest router supports case insensitive routing. This can be enabled as
follows:

```csharp
var server = new RestServer(loggerFactory)
    ...
    .ConfigureRouter(router => router.IgnoreCase = true)
    ...
```

## Simple Routes

The following registers a simple route:

```csharp
var server = new RestServer(loggerFactory)
    .AddPrefix("http://localhost:8081/")
    .ConfigureRouter(router => router.IgnoreCase = true)
    .AddRoute(SayHello, "/helloWorld", "GET")
    ...
```

This responds to the URL http://localhost:8081/helloworld. Because the
router is configured to be case insensitive it also responds to
http://localhost:8081/HelloWorld.

The example route handler was this:

```csharp
public static Task<HttpResponse> SayHello(
    RestRequest request,
    CancellationToken token)
{
    var response = HttpResponse.FromString("Hello, World!");

    return Task.FromResult(response);
}
```

The handler takes a `RestRequest` and a `CancellationToken`, and returns
a `Task<HttpResponse>`. This method is not asynchronous, so it uses
`Task.Return` to provide the task wrapped result.

The response is created with the static helper method
`HttpResponse.FromString`. The function prototype is as follows.

```csharp
public static HttpResponse FromString(
    string text,
    int statusCode = 200,
    string contentType = "text/plain",
    Encoding? contentEncoding = null,
    WebHeaderCollection? headers = null)
```

There is another convenience method `HttpResponse.FromBytes`.

```csharp
public static HttpResponse FromBytes(
    byte[] body,
    int statusCode = 200,
    string contentType = "application/octet-stream",
    Encoding? contentEncoding = null,
    WebHeaderCollection? headers = null)
```

If neither of these is suitable the class can be constructed with the
following arguments.

```csharp
public HttpResponse(
    int statusCode,
    string contentType = "text/html",
    Encoding? contentEncoding = null,
    WebHeaderCollection? headers = null,
    Stream? body = null)
```

## Query String

The next request handler gets arguments from the query string.

```csharp
public static Task<HttpResponse> SayWithQueryString(
    RestRequest request,
    CancellationToken token)
{
    var name = request.Context.Request.QueryString.Get("name");
    var age = request.Context.Request.QueryString.Get("age");

    var response = HttpResponse.FromString(
        $"Hello, {name ?? "nobody"}, you are {age ?? "a mystery"}!");

    return Task.FromResult(response);
}
```

The `Context` property of the `HttpRequest` is the `HttpListenerContext`
that was provider by `HttpListener` for the request. The context has a
`Request` property which provides the query string.

## Path Variables

The first handler that contains path variables is registered as follows.

```csharp
var server = new RestServer(loggerFactory)
    ...
    .AddRoute(SayName, "/hello/{name:string}", "GET", "POST")
    ...
```

When matched, the path segment after `hello` will be captured as a string
where the key is `name`. The handler is as follows.

```csharp
public static Task<HttpResponse> SayName(
    RestRequest request,
    CancellationToken token)
{
    var name = request.RouteInfo.Matches["name"];

    var response = HttpResponse.FromString($"Hello, {name}!");

    return Task.FromResult(response);
}
```

The variable is retrieved from the `Matches` dictionary of the `RouteInfo`
property of the request.

## Multiple Path Variables

The final handler adds a second path variable of a different type. It is
added as follows.

```csharp
var server = new RestServer(loggerFactory)
    ...
    .AddRoute(SayNameAndAge, "/hello/{name:string}/{age:int}");
```

The second path variable is an `int` and has the key `age`. The handler is
as follows.

```csharp
public static Task<HttpResponse> SayNameAndAge(
    RestRequest request,
    CancellationToken token)
{
    var name = request.RouteInfo.Matches["name"];
    var age = request.RouteInfo.Matches["age"];

    var response = HttpResponse.FromString($"Hello, {name}, you are {age}!");

    return Task.FromResult(response);
}
```
