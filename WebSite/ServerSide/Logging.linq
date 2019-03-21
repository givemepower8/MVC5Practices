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
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
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
		app.UseFileServer(
			new FileServerOptions
			{
				EnableDirectoryBrowsing = true,
				FileSystem = new PhysicalFileSystem(SampleFiles.GetHtmlsPath(Util.CurrentQueryPath))
			}
		);
		app.UseErrorPage();
	}

	private HttpConfiguration ConfigureWebApi()
	{
		var config = new HttpConfiguration();

		config.EnableCors(new EnableCorsAttribute("*", "*", "*"));

		config.MapHttpAttributeRoutes();

		config.Routes.MapHttpRoute(
			name: "DefaultApi",
			routeTemplate: "api/{controller}/{id}",
			defaults: new { id = RouteParameter.Optional }
		);

		config.EnableSwagger(c => c.SingleApiVersion("v1", "Swagger Doc")).EnableSwaggerUi();

		// by default IE will return a JSON file, to enable Ie to display json result as html
		config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

		config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

		SystemDiagnosticsTraceWriter traceWriter = config.EnableSystemDiagnosticsTracing();
		//traceWriter.IsVerbose = true;
		traceWriter.MinimumLevel = System.Web.Http.Tracing.TraceLevel.Warn;
		traceWriter.TraceSource = new TraceSource("console");

		config.MessageHandlers.Add(new LogRequestAndResponseHandler());

		var container = new UnityContainer();
		container.RegisterType<IProductRepository, ProductRepositoryStub>();
		config.DependencyResolver = new UnityDependencyResolver(container);

		return config;
	}
}

#endregion

public class LogRequestAndResponseHandler : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		string requestBody = await request.Content.ReadAsStringAsync();
		Console.WriteLine(requestBody);

		var result = await base.SendAsync(request, cancellationToken);

		if (result.Content != null)
		{
			// once response body is ready, log it
			var responseBody = await result.Content.ReadAsStringAsync();
			Console.WriteLine(responseBody);
		}

		return result;
	}
}

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
	public string GetTest(int id)
	{
		//throw new InvalidOperationException("test");
		return "test " + id.ToString();
	}
}

#endregion