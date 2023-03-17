# Getting Started

This project contains a basic server which presents one endpoint, to demonstrate
a basic HTTP server.

## Installation

The package can be installed through [nuget](https://www.nuget.org/packages/JetBlack.Http),
although this project references the local package.

## The code

There are two parts to the program. The request handler which is called
when a request in made to a endpoint, and configuring and starting the
server.

### Request handlers

Request handlers are asynchronous methods which take a *request* and a
*cancellation token* and return a *response*.

Here's an example from the code:

```csharp
public static Task<HttpResponse> IndexHandler(
    RestRequest request,
    CancellationToken token)
{
    var page = @"
<!DOCTYPE html>
<html lang='en'>
  <head>
    <meta charset='utf-8'>
    <title>title</title>
  </head>
  <body>
    <h1>Getting Started</h1>
    <p>This is a good place to start<p>
  </body>
</html>
";
    var response = HttpResponse.FromString(
        page,
        HttpStatusCode.OK,
        "text/html");

    return Task.FromResult(response);
}
```

Because this code doesn't actually do anything asynchronous it uses
`Task.FromResult`. If the handler read the page from an asynchronous
store it might look like this:

```csharp
public static async Task<HttpResponse> IndexHandler(
    RestRequest request,
    CancellationToken token)
{
    var page = await readPage("/index.html");
    return HttpResponse.FromString(
        page,
        HttpStatusCode.OK,
        "text/html");
}
```

We'll look at the request and response objects, and how to use the cancellation token later.

### Configuring and starting the server

The following code configures and starts the server:

```csharp
using (var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Trace);
}))
{
    var server = new RestServer(loggerFactory)
        .AddPrefix("http://localhost:8081/")
        .AddRoute(IndexHandler, "/index.html", "GET");

    await server.RunAsync();
}
```

The first bit of code sets up the logging handler. We don't actually
need to do this as the server will use the null logger if no factory is
provided, but it is nice to see the kind of output the logger provides.
If this was a real standalone server we might prefer to use the generic
host with an `appsettings.json` file to configure the logging. If the
server was embedded in another program, the program would most likely pass in a pre configured logging factory.

The next step is to create the HTTP server. We create a new `RestServer`
passing in the logging factory, and start configuring it with the fluent
API.

First we call `AddPrefix` to tell the listener what endpoints to listen
to. Here we have used `"http://localhost:8081/"` to use the `http` scheme,
listening to `localhost` on port `8081`. If we wanted to listen to all
interfaces the prefix would be `"http://*:8081/"` rather than `"0.0.0.0"`.
This is a quirk of the
[`HttpListener`](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener)
class on which this library is based. Also note that the prefix must end with a `/`.

Next we call `AddRoute` passing in the handler method `IndexHandler` and
the path `"/index.html"` and the HTTP method `"GET"`. The HTTP method is
of type `params string[]`, so multiple methods can be specified. The default method is `GET`, so we could have shortened this to `AddRoute(IndexHandler, "/index.html")`. 

Routing is specific to the method, so it is possible to have more than one
handler for a given path if the methods are different. For example:

```csharp
.AddRoute(GetBook, "/api/v1/book/{bookId:int}", "GET")
.AddRoute(SetBook, "/api/v1/book/{bookId:int}", "POST", "OPTIONS)
```

This uses a path that captures variables, which can be found in the
request object. We'll discuss path variables later.

Finally we run the server:

```csharp
await server.RunAsync();
```

In a real world scenario the a standalone project might use the generic server
with a background worker. An embedded project might use `Task.Run`.

At this point the server is listening for requests. Try the following
endpoint in a browser: http://localhost:8081/index.html

Next: [Configuration](../Configuration/README.md)
