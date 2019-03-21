<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <NuGetReference>Microsoft.AspNet.SignalR.Owin</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Client</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Core</NuGetReference>
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
  <Namespace>System.Web.Http</Namespace>
  <Namespace>System.Web.Http.Tracing</Namespace>
  <Namespace>Unity.WebApi</Namespace>
  <Namespace>Microsoft.Practices.Unity</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
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
		new Hyperlinq(baseUri).Dump();
		new Hyperlinq("http://localhost:5000/api/test").Dump();
		Console.WriteLine("Press Enter to quit. ", baseUri);
		Console.ReadLine();
	}
}

public class WebApiStartup
{
	public void Configuration(IAppBuilder app)
	{
		var webApiConfiguration = ConfigureWebApi();
		app.UseWebApi(webApiConfiguration);
		app.UseStaticFiles();
		app.UseErrorPage();
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

		config.MapHttpAttributeRoutes();

		// by default IE will return a JSON file, to enable Ie to display json result as html
		config.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

		config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

		SystemDiagnosticsTraceWriter traceWriter = config.EnableSystemDiagnosticsTracing();
		traceWriter.IsVerbose = true;
		//traceWriter.MinimumLevel = System.Web.Http.Tracing.TraceLevel.Warn;
		traceWriter.TraceSource = new TraceSource("console");

		config.MessageHandlers.Add(new MessageHandler1());
		config.MessageHandlers.Add(new MessageHandler2());

		return config;
	}
}

public class MessageHandler1 : DelegatingHandler
{
	protected async override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		("Before SendAsync").Dump("MessageHandler1");
		// Call the inner handler.
		var response = await base.SendAsync(request, cancellationToken);
		("After SendAsync").Dump("MessageHandler1");
		//response.Dump("MessageHandler1");
		response.Content = new StringContent(response.Content.ReadAsStringAsync().Result + "; hijacked in MessageHandler1 ");
		return response;
	}
}

public class MessageHandler2 : DelegatingHandler
{
	protected async override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		//request.GetConfiguration().
		("Before SendAsync").Dump("MessageHandler2");
		// Call the inner handler.
		var response = await base.SendAsync(request, cancellationToken);
		("After SendAsync").Dump("MessageHandler2");
		//response.Dump("MessageHandler1");
		response.Content = new StringContent(response.Content.ReadAsStringAsync().Result + " ; hijacked in MessageHandler2 ");
		return response;
	}
}

public class TestController : ApiController
{
	[HttpGet]
	//http://localhost:5000/api/test
	public string Index()
	{
		return "result in controller; ";
	}
// the above is equivelent to the following:
//	public IHttpActionResult Index()
//	{
//		return Content(HttpStatusCode.OK,"result in controller; ");
//	}
}