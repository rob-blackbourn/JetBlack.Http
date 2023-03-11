# Example of Middleware

This example demonstrates how to use middleware.

This example exposes the following endpoint:

* `http://localhost:8081/sayHello`

Middleware is provided by functions that take the request, the next handler
to call, and return the response. This allows modification of both the request
and the response. The functions are called in the order they are added.

In this example there are two middlewares:

```csharp
public static async Task<HttpResponse> FirstMiddleware(
    RestRequest request,
    Func<RestRequest, CancellationToken, Task<HttpResponse>> handler,
    CancellationToken token)
{
    Console.WriteLine(">FirstMiddleware");
    var response = await handler(request, token);
    Console.WriteLine("<FirstMiddleware");
    return response;
}

public static async Task<HttpResponse> SecondMiddleware(
    RestRequest request,
    Func<RestRequest, CancellationToken, Task<HttpResponse>> handler,
    CancellationToken token)
{
    Console.WriteLine(">SecondMiddleware");
    var response = await handler(request, token);
    Console.WriteLine("<SecondMiddleware");
    return response;
}
```

The output would be:

```
>FirstMiddleware
>SecondMiddleware
<SecondMiddleware
<FirstMiddleware
```
