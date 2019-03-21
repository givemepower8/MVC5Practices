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
  <NuGetReference>Serilog</NuGetReference>
  <NuGetReference>Serilog.Sinks.Console</NuGetReference>
  <NuGetReference>SerilogWeb.Classic</NuGetReference>
  <NuGetReference>SerilogWeb.Classic.WebApi</NuGetReference>
  <NuGetReference>Swashbuckle.Core</NuGetReference>
  <NuGetReference>Unity.WebAPI</NuGetReference>
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
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Web.Cors</Namespace>
  <Namespace>System.Web.Http</Namespace>
  <Namespace>System.Web.Http.Cors</Namespace>
  <Namespace>System.Web.Http.Tracing</Namespace>
  <Namespace>Unity</Namespace>
  <Namespace>Unity.WebApi</Namespace>
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

#define NONEST

void Main()
{
	string baseUri = "http://localhost:5000";

	using (WebApp.Start<WebApiStartup>(baseUri))
	{
		new Hyperlinq("http://localhost:5000/WebAPI2IOTests.html").Dump("Input Output tests");
		new Hyperlinq(baseUri + "/swagger/ui/index").Dump("Swagger");
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

		Serilog.Log.Logger = new LoggerConfiguration()
				.Enrich.With<WebApiRouteTemplateEnricher>()
				.Enrich.With<WebApiControllerNameEnricher>()
				.Enrich.With<WebApiRouteDataEnricher>()
				.Enrich.With<WebApiActionNameEnricher>()
				.WriteTo.Console()
				.CreateLogger();
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

		return config;
	}
}
#endregion

#region ApiController setup

[RoutePrefix("api/test")]
public class TestProductController : ApiController
{
	[HttpGet]
	[Route("")]
	//http://localhost:5000/api/test
	public IHttpActionResult Get()
	{		
		Log.Information("{@request}", this.Request);
		return Ok(new Person { FirstName = "test" });
	}
}

#endregion

public class Person
{
	public string FirstName { get; set; }
	public DateTime BirthDate {get; set;}
}