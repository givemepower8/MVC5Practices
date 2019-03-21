<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <NuGetReference>Microsoft.AspNet.SignalR.Owin</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Client</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Core</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.OwinSelfHost</NuGetReference>
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
  <Namespace>Microsoft.Owin.Hosting</Namespace>
  <Namespace>Owin</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Web.Http</Namespace>
</Query>

void Main()
{
	// Specify the URI to use for the local host:
	string baseUri = "http://localhost:5000";
	new Hyperlinq(baseUri).Dump();

	Console.WriteLine("Starting web Server...");
	using (WebApp.Start<Startup>(baseUri))
	{
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
		app.UseWelcomePage();
	}

	private HttpConfiguration ConfigureWebApi()
	{
		var config = new HttpConfiguration();
		config.Routes.MapHttpRoute(
			name: "DefaultApi",
			routeTemplate: "api/{controller}/{id}",
			defaults: new { id = RouteParameter.Optional }
		);
		
		//config.Dump();
		config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;		

		return config;
	}
}

public class Company
{
	public int Id { get; set; }
	public string Name { get; set; }
}

public class CompaniesController : ApiController
{
	// Mock a data store:
	private static List<Company> _Db = new List<Company>
			{
				new Company { Id = 1, Name = "Microsoft" },
				new Company { Id = 2, Name = "Google" },
				new Company { Id = 3, Name = "Apple" }
			};

	[HttpGet]
	public IEnumerable<Company> Get()
	{
		return _Db;
	}

	[HttpGet]
	public Company Get(int id)
	{
		var company = _Db.FirstOrDefault(c => c.Id == id);
		if (company == null)
		{
			throw new HttpResponseException(HttpStatusCode.NotFound);
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
		var companyExists = _Db.Any(c => c.Id == company.Id);

		if (companyExists)
		{
			return BadRequest("Exists");
		}

		_Db.Add(company);
		return Ok();
	}


	public IHttpActionResult Put(Company company)
	{
		if (company == null)
		{
			return BadRequest("Argument Null");
		}
		var existing = _Db.FirstOrDefault(c => c.Id == company.Id);

		if (existing == null)
		{
			return NotFound();
		}

		existing.Name = company.Name;
		return Ok();
	}

	public IHttpActionResult Delete(int id)
	{
		var company = _Db.FirstOrDefault(c => c.Id == id);
		if (company == null)
		{
			return NotFound();
		}
		_Db.Remove(company);
		return Ok();
	}
}