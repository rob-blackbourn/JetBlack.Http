# Controller With Attributes

This example demonstrates how to use a controller class with routing attributes.

Attributes were not included in the library, but they can be implemented by
downstream packages.

The attributes are used in the following manner:

```csharp
[Route("/helloWorld", "GET")]
public Task<HttpResponse> SayHello(RestRequest request, CancellationToken token)
{
    var response = HttpResponse.FromString("Hello, World!");

    return Task.FromResult(response);
}

[Route("/hello/{name:string}", "GET", "POST")]
public Task<HttpResponse> SayName(RestRequest request, CancellationToken token)
{
    var name = request.RouteInfo.Matches["name"];

    var response = HttpResponse.FromString($"Hello, {name}!");

    return Task.FromResult(response);
}
```

Next: [Middleware](../../Middleware/) or [up](..).
