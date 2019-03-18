# Web API in ASP.NET MVC 5

[ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/)

[Best practice to return errors in ASP.NET Web API
](https://stackoverflow.com/questions/10732644/best-practice-to-return-errors-in-asp-net-web-api/34890211)

## Routing

Web API exposes endpoints via routing.

[Attribute Routing](https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/create-a-rest-api-with-attribute-routing)

## Restful
- `api/resource` GET - get all
- `api/resource` POST - create new entity
- `api/resource/id` GET - get existing entity by id
- `api/resource/id` PUT - update existing entity by id
- `api/resource/id` DELETE - delete existing entity by id

The http requests and responses are processed in the action methods.

Model or parameter binding converts the parameters inside http

ASP.NET Web API converts the return value from a controller action into an HTTP response message.

A Web API controller action can return any of the following:

1. void - Return empty 204 (No Content)
2. HttpResponseMessage - an HTTP response message directly
3. IHttpActionResult - implement ExecuteAsync to create an HttpResponseMessage
4. Any other type - Write the serialized return value into the response body; return 200 (OK).

### HTTP POST Body

### DTO

## Exception Handling

- [Exception Handling in ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/overview/error-handling/exception-handling)
- [Global Error Handling in ASP.NET Web API 2](https://docs.microsoft.com/en-us/aspnet/web-api/overview/error-handling/web-api-global-error-handling)

## JavaScript client

It's done through the AJAX calls to the Restful services.

JSON formatter is used.

A typical ajax post sample
```js
var newBook = {
    Title: "title",
    Author: "author",
    Genre: "Computer",
    Price: 100,
    PublishDate: "03-01-2018",
    Description: "lorem ipsum"
}

$.ajax({
    type: "POST",
    contentType: "application/json",
    dataType: "json",
    data: JSON.stringify(newBook),
    url: '/api/books'
}).done(function (data) {
    console.log(data);
    $('#books').DataTable().draw();
}).fail(function (jqXHR, textStatus, err) {
    if (jqXHR && jqXHR.responseJSON) {
        console.log(jqXHR.responseJSON);
    }
    console.log(textStatus);
    console.log(err);
    });
});
```

GET, PUT and DELETE are similar. 

## Form post

[Sending form data on MDN](https://developer.mozilla.org/en-US/docs/Learn/HTML/Forms/Sending_and_retrieving_form_data)

[Post](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/POST)

[Form Spec](https://www.w3.org/TR/html52/sec-forms.html)