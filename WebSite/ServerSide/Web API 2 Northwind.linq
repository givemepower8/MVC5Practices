<Query Kind="Program">
  <Connection>
    <ID>70bee7de-9190-431e-9e51-9ef2883cb270</ID>
    <Persist>true</Persist>
    <Driver>EntityFrameworkDbContext</Driver>
    <CustomAssemblyPath>D:\Wei\fun.visualstudio.com\LinqPad\LinqPadFiles\Dlls\EF\Northwind.dll</CustomAssemblyPath>
    <CustomTypeName>Northwind.DAL.NorthwindContext</CustomTypeName>
    <AppConfigPath>D:\Wei\fun.visualstudio.com\LinqPad\LinqPadFiles\Dlls\EF\Northwind.dll.config</AppConfigPath>
  </Connection>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.DataAnnotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <NuGetReference>AutoMapper</NuGetReference>
  <NuGetReference>EntityFramework</NuGetReference>
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
  <NuGetReference>Serilog</NuGetReference>
  <NuGetReference>Serilog.Sinks.Console</NuGetReference>
  <NuGetReference>SerilogWeb.Classic</NuGetReference>
  <NuGetReference>SerilogWeb.Classic.WebApi</NuGetReference>
  <NuGetReference>Swashbuckle.Core</NuGetReference>
  <NuGetReference>System.Linq.Dynamic</NuGetReference>
  <NuGetReference>Unity.WebAPI</NuGetReference>
  <Namespace>AutoMapper</Namespace>
  <Namespace>Microsoft.Owin</Namespace>
  <Namespace>Microsoft.Owin.FileSystems</Namespace>
  <Namespace>Microsoft.Owin.Hosting</Namespace>
  <Namespace>Microsoft.Owin.Logging</Namespace>
  <Namespace>Microsoft.Owin.StaticFiles</Namespace>
  <Namespace>Microsoft.Practices.Unity</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Owin</Namespace>
  <Namespace>Serilog</Namespace>
  <Namespace>SerilogWeb.Classic.WebApi.Enrichers</Namespace>
  <Namespace>Swashbuckle.Application</Namespace>
  <Namespace>Swashbuckle.Swagger</Namespace>
  <Namespace>Swashbuckle.Swagger.Annotations</Namespace>
  <Namespace>Swashbuckle.Swagger.FromUriParams</Namespace>
  <Namespace>Swashbuckle.Swagger.XmlComments</Namespace>
  <Namespace>Swashbuckle.SwaggerUi</Namespace>
  <Namespace>System.ComponentModel.DataAnnotations</Namespace>
  <Namespace>System.Data.Entity</Namespace>
  <Namespace>System.Linq.Dynamic</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Formatting</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Web.Cors</Namespace>
  <Namespace>System.Web.Http</Namespace>
  <Namespace>System.Web.Http.Cors</Namespace>
  <Namespace>System.Web.Http.Dependencies</Namespace>
  <Namespace>System.Web.Http.Description</Namespace>
  <Namespace>System.Web.Http.Tracing</Namespace>
  <Namespace>Unity</Namespace>
  <Namespace>Unity.WebApi</Namespace>
  <Namespace>Northwind.Models</Namespace>
  <Namespace>System.Web.Http.ExceptionHandling</Namespace>
  <AppConfig>
    <Content>
      <configuration>
        <system.diagnostics>
          <trace autoflush="true" />
          <sources>
            <source name="console" switchValue="All">
              <listeners>
                <add name="Console" type="System.Diagnostics.ConsoleTraceListener" />
              </listeners>
            </source>
            <source name="text" switchValue="All">
              <listeners>
                <clear />
                <add name="textwriterListener" type="System.Diagnostics.TextWriterTraceListener" initializeData="C:\Temp\mylog.txt" />
              </listeners>
            </source>
          </sources>
        </system.diagnostics>
      </configuration>
    </Content>
  </AppConfig>
</Query>

#define NONEST
#define TRACE

void Main()
{	
	string baseUri = "http://localhost:5000";
	MapperConfig();
	Serilog.Log.Logger = new LoggerConfiguration()
			.WriteTo.Console()
			.CreateLogger();
	Trace.Listeners.Clear();

	using (WebApp.Start<WebApiStartup>(baseUri))
	{
		new Hyperlinq("http://localhost:5000/Northwind/DataTableDemo.html").Dump("Northwind DataTableDemo");
		new Hyperlinq(baseUri + "/swagger/ui/index").Dump("Swagger");
		Console.WriteLine("Press Enter to quit. ", baseUri);

		Console.ReadLine();
	}
}

#region WebAPI setup
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

		//config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

		config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

		SystemDiagnosticsTraceWriter traceWriter = config.EnableSystemDiagnosticsTracing();
		traceWriter.IsVerbose = true;
		traceWriter.MinimumLevel = System.Web.Http.Tracing.TraceLevel.Info;		

		// There can be multiple exception loggers. (By default, no exception loggers are registered.)
		config.Services.Add(typeof(IExceptionLogger), new MyExceptionLogger());

		config.DependencyResolver = GetUnityResolver();

		return config;
	}

	IDependencyResolver GetUnityResolver()
	{
		var container = new UnityContainer();
		// put all the type registeration here		
		return new UnityDependencyResolver(container);
	}
}

public class MyExceptionLogger : ExceptionLogger
{
	public override void Log(ExceptionLoggerContext context)
	{
		context.Dump("context");
		Serilog.Log.Logger.Fatal("{@exception}", context.Exception);
	}
}
#endregion

#region ApiController setup
[RoutePrefix("api/Category")]
public class CategoryController : ApiController
{
	[HttpGet]
	[Route("GetAllCategoriesForDataTable")]
	[ResponseType(typeof(DataTableResult))]
	//http://localhost:5000/api/Category/GetAllCategoriesForDataTable
	/// <summary>
	/// it only demonstates how to mind get query string parameters,
	/// it's not recommended, POST should be used instead
	/// </summary>
	public DataTableResult Get([FromUri]DataTableRequest request)
	{
		DataTableResult result = new DataTableResult
		{
			Draw = request.Draw
		};

		if (!ModelState.IsValid)
		{
			result.Error = String.Join(";", ModelState.Select((k, v) => { return k + " : " + v; }).ToArray());
			return result;
		}

		var total = new NorthwindServices().GetAllCategories().Count();

		var categoriesQuery = new NorthwindServices().GetAllCategories().ApplyDataTablePaging(request);

		var categorieDtos = Mapper.Map<List<CategoryDto>>(categoriesQuery.ToList());

		result = new DataTableResult
		{
			Draw = request.Draw,
			Data = categorieDtos,
			RecordsTotal = total,
			RecordsFiltered = total
		};

		return result;
	}
}

[RoutePrefix("api/Product")]
public class ProductController : ApiController
{
	[HttpPost]
	[Route("GetAllProductsForDataTable")]
	[ResponseType(typeof(DataTableResult))]
	//http://localhost:5000/api/Product/GetAllProductsForDataTable
	public DataTableResult Post(DataTableRequest request)
	{
		DataTableResult result = new DataTableResult
		{
			Draw = request.Draw
		};

		if (!ModelState.IsValid)
		{
			result.Error = String.Join(";", ModelState.Select((k, v) => { return k + " : " + v; }).ToArray());
			return result;
		}

		int total = new NorthwindServices().GetAllProducts().Count();

		var productsQuery = new NorthwindServices().GetAllProducts().ApplyDataTablePaging(request);

		var productsDtos = Mapper.Map<List<ProductDto>>(productsQuery);

		return new DataTableResult
		{
			Draw = request.Draw,
			RecordsTotal = total,
			RecordsFiltered = total,
			Data = productsDtos
		};
	}
}

#endregion

#region Dto

public class ProductDto
{
	/// <summary>
	/// Product ID
	/// </summary>
	public int ProductID { get; set; }
	public string ProductName { get; set; }
	public decimal? UnitPrice { get; set; }
	public short? UnitsInStock { get; set; }

	public CategoryDto Category { get; set; }
}

public class CategoryDto
{
	public int CategoryID { get; set; }
	public string CategoryName { get; set; }
	public string Description { get; set; }
}

public static void MapperConfig()
{
	Mapper.Initialize(cfg =>
	{
		cfg.CreateMap<Category, CategoryDto>();
		cfg.CreateMap<Product, ProductDto>();
	});

	Mapper.AssertConfigurationIsValid();
}

#endregion

#region Services
public class NorthwindServices
{
	private readonly NorthwindContext _db;

	public NorthwindServices()
	{
		_db = new NorthwindContext();
	}

	public IQueryable<Category> GetAllCategories()
	{
		return _db.Categories.Include(c => c.Products);
	}

	public IQueryable<Product> GetAllProducts()
	{
		return _db.Products.Include(p => p.Category);
	}
}
#endregion

#region DataTable related

public static class DataTableNetHelper
{
	public static IQueryable<T> ApplyDataTableFiltering<T>(this IQueryable<T> query, DataTableRequest dataTableRequest)
	{
		if (dataTableRequest == null)
		{
			throw new ArgumentNullException(nameof(dataTableRequest));
		}

		//query = query;
		return query;
	}

	public static IQueryable<T> ApplyDataTablePaging<T>(this IQueryable<T> query, DataTableRequest dataTableRequest)
	{
		if (dataTableRequest == null)
		{
			throw new ArgumentNullException(nameof(dataTableRequest));
		}

		int skip = dataTableRequest?.Start ?? 0;

		int take = dataTableRequest?.Length ?? 10;

		var order = dataTableRequest.Order.FirstOrDefault();
		if (order == null)
		{
			query = query.OrderBy(_ => 0);
		}
		else
		{
			string orderData = dataTableRequest.Columns.ElementAt(order.Column).Data;
			if (order.Dir == "asc")
				query = query.OrderBy(orderData);
			else
			{
				query = query.OrderBy(orderData + " descending");
			}
		}
		query = query.Skip(skip).Take(take);
		return query;
	}
}

public class DataTableRequestColumn
{
	[JsonProperty(PropertyName = "data")]
	public string Data { get; set; }

	[JsonProperty(PropertyName = "name")]
	public string Name { get; set; }

	[JsonProperty(PropertyName = "orderable")]
	public bool Orderable { get; set; }

	[JsonProperty(PropertyName = "search")]
	public DataTableRequestSearch Search { get; set; }

	[JsonProperty(PropertyName = "searchable")]
	public bool Searchable { get; set; }

	public DataTableRequestColumn()
	{
		Search = new DataTableRequestSearch();
	}
}

public class DataTableRequestOrder
{
	[JsonProperty(PropertyName = "column")]
	public int Column { get; set; }

	[JsonProperty(PropertyName = "dir")]
	public string Dir { get; set; }
}

public class DataTableRequestSearch
{
	[JsonProperty(PropertyName = "regex")]
	public bool Regex { get; set; }

	[JsonProperty(PropertyName = "value")]
	public string Value { get; set; }
}

public class DataTableRequest
{
	[Required]
	[JsonProperty(PropertyName = "draw")]
	/// <summary>
	/// draw number
	/// </summary>
	public int Draw { get; set; }

	[Required]
	[JsonProperty(PropertyName = "start")]
	public int? Start { get; set; }

	[Required]
	[JsonProperty(PropertyName = "length")]
	public int? Length { get; set; }

	[JsonProperty(PropertyName = "columns")]
	public List<DataTableRequestColumn> Columns { get; set; }

	[JsonProperty(PropertyName = "order")]
	public List<DataTableRequestOrder> Order { get; set; }

	[JsonProperty(PropertyName = "search")]
	public DataTableRequestSearch Search { get; set; }

	/// <summary>
	/// When we have some other search parameters not from DataTable.
	/// </summary>
	[JsonProperty(PropertyName = "parameters")]
	public List<SearchParameter> Parameters { get; set; }

	public DataTableRequest()
	{
		Columns = new List<DataTableRequestColumn>();
		Order = new List<DataTableRequestOrder>();
		Search = new DataTableRequestSearch();
		Parameters = new List<SearchParameter>();
	}
}

public class DataTableResult
{
	[JsonProperty(PropertyName = "draw")]
	public int Draw { get; set; }

	[JsonProperty(PropertyName = "recordsTotal")]
	public int RecordsTotal { get; set; }

	[JsonProperty(PropertyName = "recordsFiltered")]
	public int RecordsFiltered { get; set; }

	[JsonProperty(PropertyName = "data")]
	public IEnumerable Data { get; set; }

	[JsonProperty(PropertyName = "error")]
	public string Error { get; set; }
}

public enum SearchOperator
{
	Equal,
	NotEqual,
	GreaterThan,
	GreaterOrEqualThan,
	LessThan,
	LessOrEqualThan
};

public enum SearchLogic
{
	And,
	Or,
	Not
};

public class SearchParameter
{
	/// <summary>
	/// Gets or sets the filtering logic.
	/// </summary> 
	[JsonProperty(PropertyName = "logic")]
	public SearchLogic Logic { get; set; }

	/// <summary>
	/// Gets or sets the name of the sorted field (property).
	/// </summary>
	[JsonProperty(PropertyName = "field")]
	public string Field { get; set; }

	/// <summary>
	/// Gets or sets the filtering operator.
	/// </summary>
	[JsonProperty(PropertyName = "operator")]
	public SearchOperator Operator { get; set; }

	/// <summary>
	/// Gets or sets the filtering value. 
	/// </summary>
	[JsonProperty(PropertyName = "value")]
	public object Value { get; set; }
}
#endregion