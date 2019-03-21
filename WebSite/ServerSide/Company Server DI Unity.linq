<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <NuGetReference>Microsoft.AspNet.WebApi</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Client</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Core</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.OwinSelfHost</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.WebHost</NuGetReference>
  <NuGetReference>Swashbuckle.Core</NuGetReference>
  <NuGetReference>Unity</NuGetReference>
  <Namespace>Microsoft.Owin.Hosting</Namespace>
  <Namespace>Owin</Namespace>
  <Namespace>Swashbuckle.Application</Namespace>
  <Namespace>Swashbuckle.Swagger</Namespace>
  <Namespace>Swashbuckle.Swagger.Annotations</Namespace>
  <Namespace>Swashbuckle.Swagger.FromUriParams</Namespace>
  <Namespace>Swashbuckle.Swagger.XmlComments</Namespace>
  <Namespace>Swashbuckle.SwaggerUi</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Web.Http</Namespace>
  <Namespace>System.Web.Http.Dependencies</Namespace>
  <Namespace>Unity</Namespace>
  <Namespace>Unity.Exceptions</Namespace>
  <Namespace>Unity.Lifetime</Namespace>
</Query>

#define NONEST

void Main()
{
	// Specify the URI to use for the local host:
	string baseUri = "http://localhost:5000";

	Console.WriteLine("Starting web Server...");
	using (WebApp.Start<Startup>(baseUri))
	{
		new Hyperlinq(baseUri + "/swagger/ui/index").Dump();
		Console.WriteLine("Server running at {0} - press Enter to quit. ", baseUri);
		Console.ReadLine();
	}
}

public class Startup
{
	public void Configuration(IAppBuilder app)
	{
		var webApiConfiguration = ConfigureWebApi();
		app.UseWebApi(webApiConfiguration);
	}

	private HttpConfiguration ConfigureWebApi()
	{
		var config = new HttpConfiguration();
		config.Routes.MapHttpRoute(
			name: "DefaultApi",
			routeTemplate: "api/{controller}/{id}",
			defaults: new { id = RouteParameter.Optional }
			);

		config.EnableSwagger(c => c.SingleApiVersion("v1", "Swagger Doc")).EnableSwaggerUi();

		var container = new UnityContainer();
		container.RegisterType<ICompanyRepository, CompanyRepositoryStub>(new HierarchicalLifetimeManager());
		config.DependencyResolver = new UnityResolver(container);

		return config;
	}
}

public class Company
{
	public int Id { get; set; }
	public string Name { get; set; }
}

public interface ICompanyRepository
{
	IEnumerable<Company> GetAll();

	Company GetById(int companyId);

	bool Add(Company company);

	bool Update(Company company);

	bool Remove(int companyId);

	bool Exists(int companyId);
}

public class CompanyRepositoryStub : ICompanyRepository
{
	private List<Company> _mockDB = new List<Company>();

	public CompanyRepositoryStub()
	{
		_mockDB = new List<Company>
			{
				new Company { Id = 1, Name = "Microsoft" },
				new Company { Id = 2, Name = "Google" },
				new Company { Id = 3, Name = "Apple" }
			};
	}
	public IEnumerable<Company> GetAll()
	{
		return _mockDB;
	}

	public Company GetById(int companyId)
	{
		return _mockDB.FirstOrDefault(c => c.Id == companyId);
	}

	public bool Add(Company company)
	{
		if (Exists(company.Id))
		{
			return false;
		}
		else
		{
			_mockDB.Add(company);
			return true;
		}
	}

	public bool Update(Company company)
	{
		if (Exists(company.Id))
		{
			return false;
		}
		else
		{
			var toUpdate = _mockDB.First(c => c.Id == company.Id);
			toUpdate.Name = company.Name;
			return true;
		}
	}

	public bool Remove(int companyId)
	{
		var toRemove = _mockDB.First(c => c.Id == companyId);

		if (toRemove == null)
		{
			return false;
		}

		return _mockDB.Remove(toRemove);
	}

	public bool Exists(int companyId)
	{
		var company = _mockDB.FirstOrDefault(c => c.Id == companyId);
		return company == null;
	}
}

public class CompaniesController : ApiController
{
	private ICompanyRepository _repository;

	public CompaniesController(ICompanyRepository repository)
	{
		_repository = repository;
	}

	[HttpGet]
	public IEnumerable<Company> Get()
	{
		return _repository.GetAll();
	}

	[HttpGet]
	public Company Get(int id)
	{
		var company = _repository.GetById(id);

		if (company == null)
		{
			//return NotFound();
			// throw new HttpResponseException(HttpStatusCode.NotFound);
		}

		//RequestContext.Dump();
		return company;
	}

	public IHttpActionResult Post(Company company)
	{
		if (company == null)
		{
			return BadRequest("Argument Null");
		}

		if (!_repository.Update(company))
		{
			return BadRequest("Update failed");
		}
		else
		{
			return Ok();
		}
	}

	public IHttpActionResult Put(Company company)
	{
		if (company == null)
		{
			return BadRequest("Argument Null");
		}

		if (!_repository.Add(company))
		{
			return BadRequest("Exists");
		}
		else
		{
			return Ok();
		}
	}

	public IHttpActionResult Delete(int id)
	{
		if (!_repository.Exists(id))
		{
			return NotFound();
		}
		if (_repository.Remove(id))
		{
			return Ok();
		}
		else
		{
			return BadRequest("Delete failed");
		}
	}
}

// https://www.asp.net/web-api/overview/advanced/dependency-injection
public class UnityResolver : IDependencyResolver
{
	protected IUnityContainer container;

	public UnityResolver(IUnityContainer container)
	{
		if (container == null)
		{
			throw new ArgumentNullException("container");
		}
		this.container = container;
	}

	public object GetService(Type serviceType)
	{
		object result = null;
		try
		{
			result = container.Resolve(serviceType);
		}
		catch (ResolutionFailedException)
		{
			return null;
		}
		return result;
	}

	public IEnumerable<object> GetServices(Type serviceType)
	{
		try
		{
			return container.ResolveAll(serviceType);
		}
		catch (ResolutionFailedException)
		{
			return new List<object>();
		}
	}

	public IDependencyScope BeginScope()
	{
		var child = container.CreateChildContainer();
		return new UnityResolver(child);
	}

	public void Dispose()
	{
		container.Dispose();
	}
}