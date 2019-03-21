<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.Annotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xaml.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.Internals.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\SMDiagnostics.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Transactions.Bridge.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.Selectors.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Messaging.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.DurableInstancing.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.Activation.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <NuGetReference>EntityFramework</NuGetReference>
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
  <Namespace>Microsoft.Practices.Unity</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>Owin</Namespace>
  <Namespace>Swashbuckle.Application</Namespace>
  <Namespace>Swashbuckle.Swagger</Namespace>
  <Namespace>Swashbuckle.Swagger.Annotations</Namespace>
  <Namespace>Swashbuckle.Swagger.FromUriParams</Namespace>
  <Namespace>Swashbuckle.Swagger.XmlComments</Namespace>
  <Namespace>Swashbuckle.SwaggerUi</Namespace>
  <Namespace>System.ComponentModel.DataAnnotations</Namespace>
  <Namespace>System.Configuration</Namespace>
  <Namespace>System.Data.Entity</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Web.Http</Namespace>
  <Namespace>System.Web.Http.Tracing</Namespace>
  <Namespace>Unity.WebApi</Namespace>
  <Namespace>System.ComponentModel.DataAnnotations.Schema</Namespace>
  <Namespace>System.Data.Entity.Migrations</Namespace>
  <Namespace>System.Data.Entity.Infrastructure</Namespace>
  <Namespace>System.Web.Http.Description</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.ServiceModel.Channels</Namespace>
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
        <connectionStrings>
          <add name="BookSample" connectionString="Data Source=(localdb)\MSSQLLocalDB;Integrated Security=SSPI;Database=BookSample;" providerName="System.Data.SqlClient" />
        </connectionStrings>
      </configuration>
    </Content>
  </AppConfig>
</Query>

void Main()
{
	string baseUri = "http://localhost:5000";

	using (WebApp.Start<WebApiStartup>(baseUri))
	{
		new Hyperlinq(baseUri + "/api/test/index").Dump("index");
		new Hyperlinq(baseUri + "/swagger/ui/index").Dump("Swagger");
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
		app.UseFileServer();
		app.UseErrorPage();
	}

	private HttpConfiguration ConfigureWebApi()
	{
		var webapi = new HttpConfiguration();
		webapi.MapHttpAttributeRoutes();

		webapi.Routes.MapHttpRoute(
			name: "DefaultApi",
			routeTemplate: "api/{controller}/{id}",
			defaults: new { id = RouteParameter.Optional }
		);

		webapi.EnableSwagger(c => c.SingleApiVersion("v1", "Swagger Doc")).EnableSwaggerUi();

		webapi.MessageHandlers.Add(new MessageHandler1());
		//webapi.MessageHandlers.Add(new MessageHandler2());
		webapi.MessageHandlers.Add(new CustomHeaderHandler());
		webapi.MessageHandlers.Add(new MethodOverrideHandler());

		// by default IE will return a JSON file, to enable Ie to display json result as html
		webapi.Formatters.JsonFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));

		webapi.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always;

		SystemDiagnosticsTraceWriter traceWriter = webapi.EnableSystemDiagnosticsTracing();
		//traceWriter.IsVerbose = true;
		traceWriter.MinimumLevel = System.Web.Http.Tracing.TraceLevel.Warn;
		traceWriter.TraceSource = new TraceSource("console");

		return webapi;
	}
}

#region Controllers
public class TestController : ApiController
{
	public TestController()
	{
		Console.WriteLine("TestController in constructor");
	}

	[HttpGet]
	[Route("api/Test/index")]
	public IHttpActionResult Index()
	{
		Console.WriteLine("TestController Index is invoked");
		return Ok("hello");
	}

	[HttpGet]
	[Route("api/Test/JSONTest")]
	public IHttpActionResult JSONTest()
	{
		DateTime now = DateTime.Now;
		var anonymous = new
		{
			Test = "1",
			Now = now
		};
		return Json(anonymous);

	}
}
#endregion

#region Message Handlers
public class MessageHandler1 : DelegatingHandler
{
	protected async override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		//((HttpRequestMessageProperty)request.Properties["httpRequest"]).QueryString.Dump();
		//request.Properties["Querystring"].Dump();
		Console.WriteLine("MessageHandler1 Process request");

		// Call the inner handler.
		var response = await base.SendAsync(request, cancellationToken);

		Console.WriteLine("MessageHandler1 Process response");
		return response;
	}
}

public class MessageHandler2 : DelegatingHandler
{
	protected async override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Console.WriteLine("MessageHandler2 Process request");
		//http://localhost:7777/api/test?a
		if (((HttpRequestMessageProperty)request.Properties["httpRequest"]).QueryString == "a")
		{
			// Create the response.
			var response = new HttpResponseMessage(HttpStatusCode.OK)
			{
				Content = new StringContent("the secret place in MessageHandler2")
			};

			// Note: TaskCompletionSource creates a task that does not contain a delegate.
			var tsc = new TaskCompletionSource<HttpResponseMessage>();
			// Also sets the task state to "RanToCompletion"
			tsc.SetResult(response);

			Console.WriteLine("MessageHandler2 compelte response so no controller afterwords");
			return response;
		}
		else
		{
			// Call the inner handler.
			var continueResponse = await base.SendAsync(request, cancellationToken);
			Console.WriteLine("MessageHandler2 Process response");
			return continueResponse;
		}
	}
}

public class MethodOverrideHandler : DelegatingHandler
{
	readonly string[] _methods = { "DELETE", "HEAD", "PUT" };
	const string _header = "X-HTTP-Method-Override";

	protected override Task<HttpResponseMessage> SendAsync(
		HttpRequestMessage request, CancellationToken cancellationToken)
	{
		// Check for HTTP POST with the X-HTTP-Method-Override header.
		if (request.Method == HttpMethod.Post && request.Headers.Contains(_header))
		{
			// Check if the header value is in our methods list.
			var method = request.Headers.GetValues(_header).FirstOrDefault();
			if (_methods.Contains(method, StringComparer.InvariantCultureIgnoreCase))
			{
				// Change the request method.
				request.Method = new HttpMethod(method);
			}
		}
		return base.SendAsync(request, cancellationToken);
	}
}

public class CustomHeaderHandler : DelegatingHandler
{
	async protected override Task<HttpResponseMessage> SendAsync(
			HttpRequestMessage request, CancellationToken cancellationToken)
	{
		Console.WriteLine("CustomHeaderHandler started");
		HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

		response.Headers.Add("X-Custom-Header", "This is my custom header.");
		Console.WriteLine("CustomHeaderHandler ended");
		return response;
	}
}
#endregion