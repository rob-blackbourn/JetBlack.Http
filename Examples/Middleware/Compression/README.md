# Compression Middleware

This example demonstrates how to use the compression middleware.

The program exposes the endpoint `http://localhost:8081/quote`.

## Configuration

The middleware is configured as follows:

```csharp
var server = new RestServer(loggerFactory)
    ...
    .AddMiddleware(CompressionMiddleware<RestRouteInfo, RestServerInfo>.Create())
    ...
```

## Usage

Compression is activate through the client request headers. If the
`Accept-Encoding` header contains `gzip` or `deflate` the middleware will
automatically compress the response body.

Next: [Lifecycle](../../Lifecycle/)
