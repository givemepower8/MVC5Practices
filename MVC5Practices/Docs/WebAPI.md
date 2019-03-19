# Web API in ASP.NET MVC 5

[ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/)

[Best practice to return errors in ASP.NET Web API
](https://stackoverflow.com/questions/10732644/best-practice-to-return-errors-in-asp-net-web-api/34890211)

## Routing

Web API exposes endpoints via routing.

[Attribute Routing](https://docs.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/create-a-rest-api-with-attribute-routing)

### Restful
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

## Parameter Binding

User data is sent to action method (endpoint) via http and parameter binding.

[Parameter Binding in ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/parameter-binding-in-aspnet-web-api)


### Form post

`Content-Type: application/x-www-form-urlencoded`

[Sending form data on MDN](https://developer.mozilla.org/en-US/docs/Learn/HTML/Forms/Sending_and_retrieving_form_data)

[Post](https://developer.mozilla.org/en-US/docs/Web/HTTP/Methods/POST)

[Form Spec](https://www.w3.org/TR/html52/sec-forms.html)

### JSON POST and Body

`contentType: "application/json;charset=utf-8",`

#### AJAX client

It's done through the AJAX calls to the Restful services.

JSON formatter is used.

A typical ajax post sample
```csharp
[HttpPost]
public IHttpActionResult Post(BookDetails book)
{
    if ((ModelState.IsValid) && (book != null))
    {
        BookDetails newBook = _repository.CreateBook(book);
        if (newBook != null)
        {
            return CreatedAtRoute("DefaultApi", new {id = newBook.Id}, newBook);
        }
    }

    return BadRequest(ModelState);
            
}
```

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
    contentType: "application/json;charset=utf-8",
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

In the above sample, Post method only has one parameter so the JSON.stringify(newBook) maps to BookDetails book as long as the properties are same. If you have multiple parameters, for example, Post(BookDetails book, Orders order), then in AJAX data, `var request = {"book": {...}, "order":{...} } ` and `data: JSON.stringify(request)` will be bound to the parameters.

GET, PUT and DELETE are similar. 

### Model binding

NVC has formatters and it extracts key values and binds them to the parameters in the action method.

#### Media formatter

[Media formatter](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/media-formatters)

[JSON and XML Serialization in ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/json-and-xml-serialization)

#### Content Negotiation

[Content Negotiation in ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/content-negotiation)

### Validation

[Model Validation in ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/overview/formats-and-model-binding/model-validation-in-aspnet-web-api)

## Exception Handling

- [Exception Handling in ASP.NET Web API](https://docs.microsoft.com/en-us/aspnet/web-api/overview/error-handling/exception-handling)
- [Global Error Handling in ASP.NET Web API 2](https://docs.microsoft.com/en-us/aspnet/web-api/overview/error-handling/web-api-global-error-handling)

## Message handlers

## Response

### Built-in Results

[Built-in results](https://docs.microsoft.com/en-us/previous-versions/aspnet/dn314678%28v%3dvs.118%29)

[Help methods](https://docs.microsoft.com/en-us/dotnet/api/system.web.http.apicontroller.badrequest?view=aspnetcore-2.2)

### DTO
