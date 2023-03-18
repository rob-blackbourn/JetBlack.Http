# Cancellation Behaviour

This program demonstrates what happens when the server is stopped.

The lifetime of the server is controlled by a cancellation token that is passed
to the `RunAsync` method. This is also passed to the handlers, so they can be
aware of the lifetime.

Exposes the endpoints:

* `http://localhost:8081/quick` - responds immediately.
* `http://localhost:8081/slow` - responds after 30 seconds.

## Usage

The request handlers are passed the cancellation token when invoked. When the
token is set the server stops listening and waits for all request handlers to
finish.

The *slow* task waits for thirty seconds on the token. When then token is set
the wait exits, and the handler returns a response, rather than the connection
being dropped.

Next: [Long Running Tasks](../LongRunningHandler/) or [up](..).
