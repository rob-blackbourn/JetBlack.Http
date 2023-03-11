# Example Raw

This example builds the server from scratch using none of the fluent helper
methods.

Four endpoints are defined:

* `http://localhost:8081/api/v1/helloWorld`, `GET` - the server response with `"Hello, World!"`.
* `http://localhost:8081/api/v1/hello` - Takes the parameters as a query string,
    and demonstrates that `GET` is the default method. e.g. `http://localhost:8081/api/v1/hello?name=mary&age=12`
* `http://localhost:8081/api/v1/hello/{name:string}`, `GET` and `POST` - demonstrates
    a path with a variable and specifying multiple methods. An example might be: 
    `http://localhost:8081/api/v1/hello/mary`.
* `http://localhost:8081/api/v1/hello/{name:string}/{age:int}` - demonstrates
    multiple parameters: e.g. `http://localhost:8081/api/v1/hello/mary/12`.
