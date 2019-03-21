<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.Annotations.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Configuration.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Web.dll</Reference>
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

//https://www.asp.net/web-api/overview/web-api-routing-and-actions/create-a-rest-api-with-attribute-routing
void Main()
{
	SetupDB();
	string baseUri = "http://localhost:5000";

	using (WebApp.Start<WebApiStartup>(baseUri))
	{
		new Hyperlinq(baseUri + "/api/books").Dump("API");
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

[RoutePrefix("api/books")]
public class BooksController : ApiController
{
	private BooksAPIContext db = new BooksAPIContext();

	// Typed lambda expression for Select() method. 
	private static readonly Expression<Func<Book, BookDto>> AsBookDto =
		x => new BookDto
		{
			Title = x.Title,
			Author = x.Author.Name,
			Genre = x.Genre
		};

	// GET http://localhost:5000/api/Books
	[Route("")]
	public IQueryable<BookDto> GetBooks()
	{
		return db.Books.Include(b => b.Author).Select(AsBookDto);
	}

	// GET http://localhost:5000/api/Books/5
	[Route("{id:int}")]
	[ResponseType(typeof(BookDto))]
	public async Task<IHttpActionResult> GetBook([FromUri]int id)
	{
		BookDto book = await db.Books.Include(b => b.Author)
			.Where(b => b.BookId == id)
			.Select(AsBookDto)
			.FirstOrDefaultAsync();
		if (book == null)
		{
			return NotFound();
		}

		return Ok(book);
	}

	[Route("{id:int}/details")]
	[ResponseType(typeof(BookDetailDto))]
	public async Task<IHttpActionResult> GetBookDetail(int id)
	{
		var book = await (from b in db.Books.Include(b => b.Author)
						  where b.AuthorId == id
						  select new BookDetailDto
						  {
							  Title = b.Title,
							  Genre = b.Genre,
							  PublishDate = b.PublishDate,
							  Price = b.Price,
							  Description = b.Description,
							  Author = b.Author.Name
						  }).FirstOrDefaultAsync();

		if (book == null)
		{
			return NotFound();
		}
		return Ok(book);
	}

	[Route("{genre}")]
	public IQueryable<BookDto> GetBooksByGenre(string genre)
	{
		return db.Books.Include(b => b.Author)
			.Where(b => b.Genre.Equals(genre, StringComparison.OrdinalIgnoreCase))
			.Select(AsBookDto);
	}

	[Route("~api/authors/{authorId}/books")]
	public IQueryable<BookDto> GetBooksByAuthor(int authorId)
	{
		return db.Books.Include(b => b.Author)
			.Where(b => b.AuthorId == authorId)
			.Select(AsBookDto);
	}

	[Route("date/{pubdate:datetime:regex(\\d{4}-\\d{2}-\\d{2})}")]
	[Route("date/{*pubdate:datetime:regex(\\d{4}/\\d{2}/\\d{2})}")]
	public IQueryable<BookDto> GetBooks(DateTime pubdate)
	{
		return db.Books.Include(b => b.Author)
			.Where(b => DbFunctions.TruncateTime(b.PublishDate)
				== DbFunctions.TruncateTime(pubdate))
			.Select(AsBookDto);
	}

	protected override void Dispose(bool disposing)
	{
		db.Dispose();
		base.Dispose(disposing);
	}
}

#region Model
public class Author
{
	public int AuthorId { get; set; }
	[Required]
	public string Name { get; set; }
}

public class Book
{
	public int BookId { get; set; }
	[Required]
	public string Title { get; set; }
	public decimal Price { get; set; }
	public string Genre { get; set; }
	public DateTime PublishDate { get; set; }
	public string Description { get; set; }
	public int AuthorId { get; set; }
	[ForeignKey("AuthorId")]
	public Author Author { get; set; }
}
#endregion

#region Dtos
public class BookDto
{
	public string Title { get; set; }
	public string Author { get; set; }
	public string Genre { get; set; }
}

public class BookDetailDto
{
	public string Title { get; set; }
	public string Genre { get; set; }
	public DateTime PublishDate { get; set; }
	public string Description { get; set; }
	public decimal Price { get; set; }
	public string Author { get; set; }
}
#endregion

#region Database Setup
public class BooksAPIContext : DbContext
{
	public DbSet<Book> Books { get; set; }
	public DbSet<Author> Authors { get; set; }

	public BooksAPIContext() : base(ConfigurationManager.ConnectionStrings["BookSample"].ConnectionString)
	{

	}
}

public class BooksAPIContextInitializer : DropCreateDatabaseAlways<BooksAPIContext>
{
	protected override void Seed(BooksAPIContext context)
	{
		context.Authors.AddOrUpdate(
			new Author[] {
				new Author() { AuthorId = 1, Name = "Ralls, Kim" },
				new Author() { AuthorId = 2, Name = "Corets, Eva" },
				new Author() { AuthorId = 3, Name = "Randall, Cynthia" },
				new Author() { AuthorId = 4, Name = "Thurman, Paula" }
			}
		);

		context.Books.AddOrUpdate(
			new Book[] {
				new Book() { BookId = 1,  Title= "Midnight Rain", Genre = "Fantasy",
				PublishDate = new DateTime(2000, 12, 16), AuthorId = 1, Description =
				"A former architect battles an evil sorceress.", Price = 14.95M },

				new Book() { BookId = 2, Title = "Maeve Ascendant", Genre = "Fantasy",
					PublishDate = new DateTime(2000, 11, 17), AuthorId = 2, Description =
					"After the collapse of a nanotechnology society, the young" +
					"survivors lay the foundation for a new society.", Price = 12.95M },

				new Book() { BookId = 3, Title = "The Sundered Grail", Genre = "Fantasy",
					PublishDate = new DateTime(2001, 09, 10), AuthorId = 2, Description =
					"The two daughters of Maeve battle for control of England.", Price = 12.95M },

				new Book() { BookId = 4, Title = "Lover Birds", Genre = "Romance",
					PublishDate = new DateTime(2000, 09, 02), AuthorId = 3, Description =
					"When Carla meets Paul at an ornithology conference, tempers fly.", Price = 7.99M },

				new Book() { BookId = 5, Title = "Splish Splash", Genre = "Romance",
					PublishDate = new DateTime(2000, 11, 02), AuthorId = 4, Description =
					"A deep sea diver finds true love 20,000 leagues beneath the sea.", Price = 6.99M}
			}
		);
	}
}

public void SetupDB()
{
	Database.SetInitializer(new BooksAPIContextInitializer());
	using (var db = new BooksAPIContext())
	{
		db.Authors.Count().Dump("Authors.Count()");
	}
}
#endregion