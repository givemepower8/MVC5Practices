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

In Web API 2, if controller throws an uncaught exception, by default, most exceptions are translated into an HTTP response with status code 500, Internal Server Error.

You can customize how Web API handles exceptions by writing an exception filter. It can be action-specific, controller-specific or global scope. An exception filter is executed when a controller method throws any unhandled exception that is not an HttpResponseException exception. The common usuage is when we need to log some specific exceptions. Instead of writting duplicated logging in the methods, we can handle it in a more maintainable way.

But there are a number of cases that exception filters can't handle. For example:

- Exceptions thrown from controller constructors.
- Exceptions thrown from message handlers.
- Exceptions thrown during routing.
- Exceptions thrown during response content serialization.

 IExceptionLogger and IExceptionHandler, are two user-replaceable services to log and handle unhandled exceptions. 

- Exception loggers are the solution to seeing all unhandled exception caught by Web API.
- Exception handlers are the solution for customizing all possible responses to unhandled exceptions caught by Web API.
- Exception filters are the easiest solution for processing the subset unhandled exceptions related to a specific action or controller.

```csharp
public interface IExceptionLogger
{
   Task LogAsync(ExceptionLoggerContext context, 
                 CancellationToken cancellationToken);
}

public interface IExceptionHandler
{
   Task HandleAsync(ExceptionHandlerContext context, 
                    CancellationToken cancellationToken);
}
```

Example
```csharp
public class SlabLogExceptionLogger : ExceptionLogger
{
    public override void Log(ExceptionLoggerContext context)
    {
        TestEvents.Log.UnhandledException(
            context.Request.Method.ToString(),  
            context.Request.RequestUri.ToString(), 
            context.Exception.Message);
    }
}

// in the WebApiConfig Register
config.Services.Add(typeof(IExceptionLogger),  new SlabLogExceptionLogger());
```

HttpResponseException is a special case. It's not a real exception which you can use try catch. It is designed specifically for returning an HTTP response in the pipeline.

```csharp
public IEnumerable<string> GetBasicException()
{
    throw new HttpResponseException(HttpStatusCode.Moved);
    return new string[] { "value1", "value2" };
}
```

The client will immediately get a 301 moved response.

```csharp
var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
{
    Content = new StringContent(string.Format("id > 3, your value: {0}", id)),
    ReasonPhrase = "Your id is too big"
};
throw new HttpResponseException(resp);
```

It can be used in the exp

```csharp
public class ValidationExceptionFilterAttribute : ExceptionFilterAttribute 
{
    public override void OnException(HttpActionExecutedContext context)
    {
        if (context.Exception is ValidationException)
        {
            var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(context.Exception.Message),
                ReasonPhrase = "ValidationException"
            };
            throw new HttpResponseException(resp);
        }
    }
}
```

The HttpError object provides a consistent way to return error information in the response body. 
```csharp
 
var message = string.Format("id > 3 {0} ", id);
HttpError err = new HttpError(message);
err["error_id_Validation"] = 300;
return Request.CreateResponse(HttpStatusCode.BadRequest, err);

```
## Message handlers

## Action Filters

[Web API 2 Using ActionFilterAttribute](https://damienbod.com/2014/01/04/web-api-2-using-actionfilterattribute-overrideactionfiltersattribute-and-ioc-injection/)

## Response

### Built-in Results

[Built-in results](https://docs.microsoft.com/en-us/previous-versions/aspnet/dn314678%28v%3dvs.118%29)

[Help methods](https://docs.microsoft.com/en-us/dotnet/api/system.web.http.apicontroller.badrequest?view=aspnetcore-2.2)

### DTO

## Troubleshooting

### Logging

In ASP.NET MVC 5, ILogger is not abstracted like ASP.NET Core which is very flexible. If you are not switching logging providers later on, just stick to one provider and use singlton to reference the static logger.

## Documention

### Swagger

[swagger-net](https://www.nuget.org/packages/Swagger-Net/) for ASP.NET Web API 2

- [Swagger and ASP.NET Web API - Part 1](http://wmpratt.com/swagger-and-asp-net-web-api-part-1/)
- [Swagger and ASP.NET Web API - Part 2](http://wmpratt.com/part-ii-swagger-and-asp-net-web-api-enabling-oauth2/)

To use xml commment, in Project -> Build, specify the XML Documentation file. in SwaggerConfig.cs, there is ` c.IncludeAllXmlComments(thisAssembly, AppDomain.CurrentDomain.BaseDirectory);` which checks the project root folder and apply the xml dcomenention to action methods.

## Resources

### Offical

- [ASP.NET Web API 2](https://docs.microsoft.com/en-us/aspnet/web-api/overview/)
- [ASP.NET Web API 2 Samples](https://github.com/aspnet/samples/tree/master/samples/aspnet/WebApi)