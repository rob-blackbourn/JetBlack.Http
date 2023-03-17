# Overview

This example demonstrates how to use middleware.

This example exposes the following endpoint:

* `http://localhost:8081/sayHello`

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
