<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <NuGetReference>Microsoft.AspNet.SignalR.Owin</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Client</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Core</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Cors</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.OwinSelfHost</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Tracing</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.WebHost</NuGetReference>
  <NuGetReference>Microsoft.Owin.Cors</NuGetReference>
  <NuGetReference>Microsoft.Owin.Diagnostics</NuGetReference>
  <NuGetReference>Microsoft.Owin.FileSystems</NuGetReference>
  <NuGetReference>Microsoft.Owin.Host.HttpListener</NuGetReference>
  <NuGetReference>Microsoft.Owin.Hosting</NuGetReference>
  <NuGetReference>Microsoft.Owin.Security</NuGetReference>
  <NuGetReference>Microsoft.Owin.Security.Facebook</NuGetReference>
  <NuGetReference>Microsoft.Owin.Security.Google</NuGetReference>
  <NuGetReference>Microsoft.Owin.Security.MicrosoftAccount</NuGetReference>
  <NuGetReference>Microsoft.Owin.Security.OAuth</NuGetReference>
  <NuGetReference>Microsoft.Owin.StaticFiles</NuGetReference>
  <NuGetReference>Swashbuckle.Core</NuGetReference>
  <NuGetReference>Unity.WebAPI</NuGetReference>
  <Namespace>Microsoft.Owin.FileSystems</Namespace>
  <Namespace>Microsoft.Owin.Hosting</Namespace>
  <Namespace>Microsoft.Owin.StaticFiles</Namespace>
  <Namespace>Microsoft.Practices.Unity</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Owin</Namespace>
  <Namespace>Swashbuckle.Application</Namespace>
  <Namespace>Swashbuckle.Swagger</Namespace>
  <Namespace>Swashbuckle.Swagger.Annotations</Namespace>
  <Namespace>Swashbuckle.Swagger.FromUriParams</Namespace>
  <Namespace>Swashbuckle.Swagger.XmlComments</Namespace>
  <Namespace>Swashbuckle.SwaggerUi</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Web.Cors</Namespace>
  <Namespace>System.Web.Http</Namespace>
  <Namespace>System.Web.Http.Cors</Namespace>
  <Namespace>System.Web.Http.Tracing</Namespace>
  <Namespace>Unity.WebApi</Namespace>
  <Namespace>System.Web.Http.ExceptionHandling</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web</Namespace>
  <Namespace>System.Web.Http.Dispatcher</Namespace>
  <Namespace>System.Web.Http.Controllers</Namespace>
  <Namespace>Unity</Namespace>
  <AppConfig>
    <Content>
      <configuration>
        <system.diagnostics>
          <sources>
            <source name="console" switchValue="All">
              <listeners>
                <add name="Console" type="System.Diagnostics.ConsoleTraceListener" />
              </listeners>
            </source>
          </sources>
        </system.diagnostics>
      </configuration>
    </Content>
  </AppConfig>
</Query>

//https://www.asp.net/web-api/overview/error-handling/web-api-global-error-handling
void Main()
{
	string baseUri = "http://localhost:5000";

	using (WebApp.Start<WebApiStartup>(baseUri))
	{
		new Hyperlinq(baseUri).Dump("Static files");
		new Hyperlinq(baseUri + "/swagger/ui/index").Dump("Swagger");
		new Hyperlinq(baseUri + "/WebApiClients/ProductClient.html").Dump("ProductClient in jQuery");
		Console.WriteLine("Press Enter to quit. ", baseUri);
		Console.ReadLine();
	}
}

#region API setup

public class WebApiStartup
{
	public void Configuration(IAppBuilder app)
	{
		var webApiConfiguration = ConfigureWebApi();
		app.UseWebApi(webApiConfiguration);
		app.UseStaticFiles();
		app.UseFileServer();
		app.UseErrorPage();
	}

	private HttpConfiguration ConfigureWebApi()
	{
		var config = new HttpConfiguration();

		config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

		config.MapHttpAttributeRoutes();
		
		config.Routes.MapHttpRoute(
			name: "Error404",
			routeTemplate: "api/{*uri}",
			defaults: new { controller = "Error", action = "Handle404" }
		);
		
		config.Routes.MapHttpRoute(
			name: "DefaultApi",
			routeTemplate: "api/{controller}/{id}",
			defaults: new { id = RouteParameter.Optional }
		);
		
		config.EnableSwagger(c => c.SingleApiVersion("v1", "Swagger Doc")).EnableSwaggerUi();

		// by default IE will return a JSON file, to enable Ie to display json result as html
		config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

		config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

//		SystemDiagnosticsTraceWriter traceWriter = webapi.EnableSystemDiagnosticsTracing();
//		//traceWriter.IsVerbose = true;
//		traceWriter.MinimumLevel = System.Web.Http.Tracing.TraceLevel.Warn;
//		traceWriter.TraceSource = new TraceSource("console");

		var container = new UnityContainer();
		container.RegisterType<IProductRepository, ProductRepositoryStub>();
		config.DependencyResolver = new UnityDependencyResolver(container);

		// There can be multiple exception loggers. (By default, no exception loggers are registered.)
		config.Services.Add(typeof(IExceptionLogger), new ExceptionLogger());

		// There must be exactly one exception handler. (There is a default one that may be replaced.)
		// To make this sample easier to run in a browser, replace the default exception handler with one that sends
		// back text/plain content for all errors.
		config.Services.Replace(typeof(IExceptionHandler), new ExceptionHandler());

		return config;
	}
}

#endregion

#region Database Mock setup
public class Product
{
	public int? Id { get; set; }
	public string Name { get; set; }
	public decimal Price { get; set; }
}

public interface IProductRepository
{
	IEnumerable<Product> GetAllProducts();

	Product GetProductById(int productId);

	Product Add(Product productToAdd);

	bool Update(int productId, Product productToUpdate);

	bool Remove(int productId);
}

public class ProductRepositoryStub : IProductRepository
{
	private static List<Product> _products = new List<Product>();

	static ProductRepositoryStub()
	{
		_products = new List<Product>
		{
			new Product { Id = 1, Name = "IPhone 7", Price = 1500 },
			new Product { Id = 2, Name = "Samsung Edge S6", Price = 1000},
			new Product { Id = 3, Name = "Google Pixel", Price = 800 }
		};
	}

	public IEnumerable<Product> GetAllProducts()
	{
		return _products;
	}

	public Product GetProductById(int productId)
	{
		return _products.FirstOrDefault(c => c.Id == productId);
	}

	public Product Add(Product product)
	{
		if (product == null)
		{
			throw new ArgumentNullException();
		}
		if (product.Id.HasValue && GetProductById(product.Id.Value) != null)
		{
			return null;
		}
		product.Id = _products.Count() + 1;
		_products.Add(product);

		return product;
	}

	public bool Update(int id, Product product)
	{
		if (GetProductById(id) == null)
		{
			return false;
		}
		else
		{
			var toUpdate = _products.First(c => c.Id == id);
			toUpdate.Name = product.Name;
			return true;
		}
	}

	public bool Remove(int productId)
	{
		var toRemove = GetProductById(productId);

		if (toRemove == null)
		{
			return false;
		}

		return _products.Remove(toRemove);
	}

}
#endregion

#region ApiController setup

[RoutePrefix("api/product")]
//[EnableCors(origins: "*", headers: "*", methods: "*")]
public class ProductController : ApiController
{
	private IProductRepository _repository;

	public ProductController(IProductRepository repository)
	{
		_repository = repository;
	}

	[HttpGet]
	[Route("")]
	//http://localhost:5000/api/product
	public IEnumerable<Product> Get()
	{
		return _repository.GetAllProducts();
	}

	[HttpGet]
	[Route("{id:int}")]
	//http://localhost:5000/api/product/1
	public IHttpActionResult Get(int id)
	{
		var product = _repository.GetProductById(id);

		if (product == null)
		{
			return NotFound();
		}

		return Ok(product);
	}

	[HttpPost] //create new entity
	[Route("")]
	public IHttpActionResult Post(Product product)
	{
		if (product == null)
		{
			return BadRequest("Argument Null");
		}

		var newProduct = _repository.Add(product);
		if (newProduct != null)
		{
			return Created(Request.RequestUri + "/" + product.Id.ToString(), newProduct);
		}
		else
		{
			return Conflict();
		}
	}

	[HttpPut] // update Product
	[Route("{id:int}")]
	//http://localhost:5000/api/product/1
	public IHttpActionResult Put(int id, Product product)
	{
		if (product == null)
		{
			return BadRequest("Argument Null");
		}

		var existing = _repository.GetProductById(id);

		if (existing == null)
		{
			return NotFound();
		}

		return Ok(_repository.Update(id, product));

	}

	[HttpDelete]
	[Route("{id:int}")]
	public IHttpActionResult Delete(int id)
	{
		var product = _repository.GetProductById(id);
		if (product == null)
		{
			return NotFound();
		}
		return Ok(_repository.Remove(id));
	}

	// http://localhost:5000/api/product/1/test
	[HttpGet]
	[Route("{id}/test")]
	public IHttpActionResult GetTest(int id)
	{
		//return NotFound();
		throw new InvalidOperationException("test");
		return Ok();
		//return "test " + id.ToString();
	}
}

#endregion

public class ExceptionLogger : IExceptionLogger
{
	public virtual Task LogAsync(ExceptionLoggerContext context,
								 CancellationToken cancellationToken)
	{
		if (!ShouldLog(context))
		{
			return Task.FromResult(0);
		}

		return LogAsyncCore(context, cancellationToken);
	}

	public virtual Task LogAsyncCore(ExceptionLoggerContext context,
									 CancellationToken cancellationToken)
	{
		LogCore(context);
		return Task.FromResult(0);
	}

	public virtual void LogCore(ExceptionLoggerContext context)
	{
		File.AppendAllText(@"C:\TEMP\errorlog.txt",context.RequestContext.Url.ToString());
	}

	public virtual bool ShouldLog(ExceptionLoggerContext context)
	{
		IDictionary exceptionData = context.ExceptionContext.Exception.Data;

		if (!exceptionData.Contains("MS_LoggedBy"))
		{
			exceptionData.Add("MS_LoggedBy", new List<object>());
		}

		ICollection<object> loggedBy = ((ICollection<object>)exceptionData["MS_LoggedBy"]);

		if (!loggedBy.Contains(this))
		{
			loggedBy.Add(this);
			return true;
		}
		else
		{
			return false;
		}
	}
}

public class ExceptionHandler : IExceptionHandler
{
	public virtual Task HandleAsync(ExceptionHandlerContext context,
									CancellationToken cancellationToken)
	{
		if (!ShouldHandle(context))
		{
			return Task.FromResult(0);
		}

		return HandleAsyncCore(context, cancellationToken);
	}

	public virtual Task HandleAsyncCore(ExceptionHandlerContext context,
									   CancellationToken cancellationToken)
	{
		HandleCore(context);
		return Task.FromResult(0);
	}

	public virtual void HandleCore(ExceptionHandlerContext context)
	{
	}

	public virtual bool ShouldHandle(ExceptionHandlerContext context)
	{
		return context.ExceptionContext.CatchBlock.IsTopLevel;
	}
}

public class ErrorController : ApiController
{
	[HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead, HttpOptions, AcceptVerbs("PATCH")]
	public HttpResponseMessage Handle404()
	{
		throw new InvalidOperationException("not found");
		var responseMessage = new HttpResponseMessage(HttpStatusCode.NotFound);
		responseMessage.ReasonPhrase = "The requested resource is not found";
		return responseMessage;
	}
}