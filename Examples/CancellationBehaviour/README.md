# Cancellation Behaviour

This program demonstrates what happens when the server is stopped.

The lifetime of the server is controlled by a cancellation token that is passed
to the `RunAsync` method. This is also passed to the handlers, so they can be
aware of the lifetime.
