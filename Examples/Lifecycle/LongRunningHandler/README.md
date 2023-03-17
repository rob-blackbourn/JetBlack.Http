# Long Running Handler

This example demonstrates that long running handlers don't prevent other
handlers from running.

There are two endpoints:

* quick: http://localhost/quick
* slow: http://localhost/quick

The "quick" endpoint returns immediately, while the "slow" endpoint sleeps for
30 seconds (simulating a long running handler).

## Instructions

1. Open a browser and in the first tab enter the quick end point. It should
respond with the word "Quick" and the time.
2. Now open a second browser and enter the slow endpoint. The browser should
indicate it is waiting.
3. Go back to the quick tab and refresh. The time should change, indicating
the quick endpoint is not blocked by the slow endpoint.

Next: [up](..).
