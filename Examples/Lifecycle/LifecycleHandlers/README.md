# Lifecycle Handlers

This example demonstrates the use of lifecycle handlers.

Lifecycle handlers are run at server startup and shutdown. They are called in
the order they were added.

These handlers provide ways to manage resources in the scope of the lifetime
of the server. For example creating a database connection.

A startup handler looks like this:

```csharp
public static Task FirstStartupHandler(RestServerInfo serverInfo, CancellationToken token)
{
    Console.WriteLine("FirstStartupHandler");

    return Task.CompletedTask;
}
```

A shutdown handler is similar, but omits the cancellation token, as that will
always be set.

```csharp
public static Task FirstShutdownHandler(RestServerInfo serverInfo)
{
    Console.WriteLine("FirstShutdownHandler");

    return Task.CompletedTask;
}
```

Next: [up](..).
