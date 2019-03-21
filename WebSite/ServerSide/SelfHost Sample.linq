<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.Internals.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.DurableInstancing.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Runtime.Serialization.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Xaml.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\SMDiagnostics.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.EnterpriseServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.Services.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.ApplicationServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceProcess.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Messaging.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.IdentityModel.Selectors.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.DirectoryServices.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\Microsoft.Transactions.Bridge.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ServiceModel.Activation.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Security.dll</Reference>
  <GACReference>System.Web.Helpers, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</GACReference>
  <GACReference>System.Web.WebPages, Version=2.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35</GACReference>
  <NuGetReference>Microsoft.AspNet.WebApi.Cors</NuGetReference>
  <NuGetReference>Microsoft.AspNet.WebApi.SelfHost</NuGetReference>
  <NuGetReference>Microsoft.Net.Http</NuGetReference>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Web.Cors</Namespace>
  <Namespace>System.Web.Http</Namespace>
  <Namespace>System.Web.Http.SelfHost</Namespace>
  <Namespace>System.Web.Http.Tracing</Namespace>
  <Namespace>System.Web.Http.Cors</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.ServiceModel.Channels</Namespace>
</Query>

// You need to run LinqPad with the administrator
void Main()
{	
	TurnOnHttpServer();
}

public void TurnOnHttpServer()
{
	HttpSelfHostConfiguration config = GetConfig();
	HttpSelfHostServer server = null;
	
	try
	{	
		server = new HttpSelfHostServer(config);
		server.OpenAsync().Wait();
		new Hyperlinq("http://localhost:5000" + "/api/test/index").Dump("Index");

		Util.ReadLine("Server is runing. Press any key to quit.");	
	}
	
	catch (Exception ex)
	{		
		Console.WriteLine("Could not start server");
		Console.WriteLine("Exception Message {0}", ex.Message);
		Console.WriteLine("Base Exception Message {0}", ex.GetBaseException().Message);
		Console.WriteLine("Inner Exception Message: {0}", ex.InnerException.Message);
	}
	
	finally
	{
		if (server != null)
		{		
			server.CloseAsync().Wait();
		}
	}
}

static string url = "http://localhost:5000";

public HttpSelfHostConfiguration GetConfig()
{
	HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(url);		
		
	config.Routes.MapHttpRoute(
		"DefaultApi", "api/{controller}/{action}/{id}", 
		new { id = RouteParameter.Optional });
	
	config.EnableCors(new EnableCorsAttribute("*", "*", "*"));
	return config;
}

#region Controllers
public class TestController : ApiController
{	
	public TestController ()
	{		
		Console.WriteLine("TestController in constructor");
	}
	
	[HttpGet]
	//http://localhost:5000/api/test/index
	public IHttpActionResult Index()
	{
		Console.WriteLine("TestController Index is invoked");
		return new TextResult("hello", Request);		
	}
		
	[HttpGet]
	[Route("api/Test/JSONTest")]
	//http://localhost:5000/api/test/JSONTest
	public IHttpActionResult JSONTest()
	{
		DateTime now = DateTime.Now;
		var anonymous = new {
			Test = "1",
			Now = now
		};
		return Json(anonymous);
		
	}
}
#endregion

public class TextResult : IHttpActionResult
{
    string _value;
    HttpRequestMessage _request;

    public TextResult(string value, HttpRequestMessage request)
    {
        _value = value;
        _request = request;
    }
    public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage()
        {
            Content = new StringContent(_value),
            RequestMessage = _request
        };
        return Task.FromResult(response);
    }
}