<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.ComponentModel.Annotations.dll</Reference>
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
  <NuGetReference>System.Linq.Dynamic</NuGetReference>
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
  <Namespace>System.ComponentModel.DataAnnotations.Schema</Namespace>
  <Namespace>System.Data.Entity</Namespace>
  <Namespace>System.Data.Entity.ModelConfiguration</Namespace>
  <Namespace>System.Linq.Dynamic</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Net.Http.Headers</Namespace>
  <Namespace>System.Web.Cors</Namespace>
  <Namespace>System.Web.Http</Namespace>
  <Namespace>System.Web.Http.Cors</Namespace>
  <Namespace>System.Web.Http.ModelBinding</Namespace>
  <Namespace>System.Web.Http.Tracing</Namespace>
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

void Main()
{
	string baseUri = "http://localhost:5000";

	using (WebApp.Start<WebApiStartup>(baseUri))
	{
		new Hyperlinq(baseUri).Dump("Static files");
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

		var container = new UnityContainer();
		container.RegisterType<ISampleProductRepository, SampleProductRepositoryStub>();
		config.DependencyResolver = new UnityDependencyResolver(container);

		config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling
	   = Newtonsoft.Json.ReferenceLoopHandling.Ignore;

		return config;
	}
}

#endregion

#region ApiController setup

[RoutePrefix("northwind")]
public class NorthwindController : ApiController
{
	[HttpGet]
	[Route("customers")]
	//http://localhost:5000/northwind/customers
	public DataTableResult GetCustomers()
	{
		var defaultTake = 10;

		List<Customer> list = new List<Customer>();
		using (var db = new NorthwindContext(Constants.DB.Northwind2014))
		{
			list = db.Customers.Take(defaultTake).ToList();
		}

		var result = new DataTableResult();

		result.Draw = 1;
		result.RecordsFiltered = 0;
		result.RecordsTotal = defaultTake;
		result.Data = list;
		return result;
	}

	[HttpPost]
	[Route("customers")]
	//http://localhost:5000/northwind/customers
	public DataTableResult GetAllCustomersByRequest(DataTableRequestDetail request)
	{
		// request.Dump();
		List<Customer> list = new List<Customer>();
		int total;

		string orderBy = request.Columns[request.Orders[0].Column].Data;
		bool orderAscending = request.Orders[0].Direction == "asc" ? true : false;

		using (var db = new NorthwindContext(Constants.DB.Northwind2014))
		{
			var query = from customer in db.Customers.Include(c=>c.Orders)
						select customer;
			total = query.Count();
			if (orderAscending)
			{				
				query = query.OrderBy(orderBy);
			}
			else
			{
				query = query.OrderBy(orderBy + " descending"); //https://stackoverflow.com/questions/9686931/system-linq-dynamic-does-not-support-orderbydescendingsomecolumn				
			}

			list = query.Skip(request.Start).Take(request.Length).ToList();
		}

		var result = new DataTableResult();

		result.RecordsFiltered = total;
		result.RecordsTotal = total;
		result.Data = list;
		//result.Error = "error test";
		return result;
	}


	[HttpGet]
	[Route("employees")]
	//http://localhost:5000/northwind/employees
	public DataTableResult GetAllEmployees()
	{
		var result = new DataTableResult();
		List<Employee> list = new List<Employee>();
		using (var db = new NorthwindContext(Constants.DB.Northwind2014))
		{
			list = db.Employees.ToList();
		}
		result.Data = list;
		return result;
	}

	[HttpGet]
	[Route("categories")]
	//http://localhost:5000/northwind/categories
	public DataTableResult GetAllCategories()
	{
		var result = new DataTableResult();
		List<Category> list = new List<Category>();
		using (var db = new NorthwindContext(Constants.DB.Northwind2014))
		{
			list = db.Categories.ToList();
		}
		result.Data = list;
		return result;
	}
}

#endregion

#region Infrastructure
public class SearchRequest
{
	[JsonProperty(PropertyName = "search")]
	public SearchRequestDetail Search { get; set; }
}

public class SearchRequestDetail
{
	[JsonProperty(PropertyName = "skip")]
	// Though the framework has named this property skip, it actually means page number
	public int Skip { get; set; }

	[JsonProperty(PropertyName = "take")]
	public int Take { get; set; }

	[JsonProperty(PropertyName = "filters")]
	public List<SearchRequestFilter> Filters { get; set; }

	[JsonProperty(PropertyName = "sorts")]
	public List<SearchRequestSort> Sorts { get; set; }

	public SearchRequestDetail()
	{
		Filters = new List<SearchRequestFilter>();
		Sorts = new List<SearchRequestSort>();
	}
}

public class SearchRequestFilter
{
	/// <summary>
	/// Gets or sets the filtering logic. Can be set to "or" or "and".
	/// </summary> 
	[JsonProperty(PropertyName = "logic")]
	public string Logic { get; set; }

	/// <summary>
	/// Gets or sets the name of the sorted field (property).
	/// </summary>
	[JsonProperty(PropertyName = "field")]
	public string Field { get; set; }

	/// <summary>
	/// Gets or sets the filtering operator.
	/// </summary>
	[JsonProperty(PropertyName = "operator")]
	public string Operator { get; set; }

	/// <summary>
	/// Gets or sets the filtering value. 
	/// </summary>
	[JsonProperty(PropertyName = "value")]
	public object Value { get; set; }
}

public class SearchRequestSort
{
	public string Field { get; set; }

	public string Direction { get; set; }
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
}

public class DataTableRequestOrder
{
	[JsonProperty(PropertyName = "column")]
	public int Column { get; set; }

	[JsonProperty(PropertyName = "dir")]
	public string Direction { get; set; }
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
	[JsonProperty(PropertyName = "request")]
	public DataTableRequestDetail request { get; set; }
}

public class DataTableRequestDetail
{
	[JsonProperty(PropertyName = "columns")]
	public List<DataTableRequestColumn> Columns { get; set; }

	[JsonProperty(PropertyName = "draw")]
	public int Draw { get; set; }

	[JsonProperty(PropertyName = "length")]
	public int Length { get; set; }

	[JsonProperty(PropertyName = "order")]
	public List<DataTableRequestOrder> Orders { get; set; }

	[JsonProperty(PropertyName = "search")]
	public DataTableRequestSearch Search { get; set; }

	[JsonProperty(PropertyName = "start")]
	public int Start { get; set; }
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
#endregion

#region Northwind DBContext and setup

public class NorthwindContext : DbContext
{
	public NorthwindContext() : base("Northwind")
	{		
	}

	public NorthwindContext(string connString)
		: base(connString)
	{
		//this.Database.Log = Console.WriteLine;
		Configuration.ProxyCreationEnabled = false;
	}

	public DbSet<Category> Categories { get; set; }
	public DbSet<Customer> Customers { get; set; }
	public DbSet<Employee> Employees { get; set; }
	public DbSet<OrderDetail> OrderDetails { get; set; }
	public DbSet<Order> Orders { get; set; }
	public DbSet<Product> Products { get; set; }
	public DbSet<Region> Regions { get; set; }
	public DbSet<Shipper> Shippers { get; set; }
	public DbSet<Supplier> Suppliers { get; set; }
	public DbSet<Territory> Territories { get; set; }
	public DbSet<Invoice> Invoices { get; set; }

	protected override void OnModelCreating(DbModelBuilder modelBuilder)
	{
		modelBuilder.Configurations.Add(new CategoryMap());
		modelBuilder.Configurations.Add(new CustomerMap());
		modelBuilder.Configurations.Add(new EmployeeMap());
		modelBuilder.Configurations.Add(new OrderDetailMap());
		modelBuilder.Configurations.Add(new OrderMap());
		modelBuilder.Configurations.Add(new ProductMap());
		modelBuilder.Configurations.Add(new RegionMap());
		modelBuilder.Configurations.Add(new ShipperMap());
		modelBuilder.Configurations.Add(new SupplierMap());
		modelBuilder.Configurations.Add(new TerritoryMap());
		modelBuilder.Configurations.Add(new InvoiceMap());
	}
}

#region Models
public class Category
{
	private ICollection<Product> _products;

	public Category()
	{
		_products = new List<Product>();
	}

	public int CategoryID { get; set; }

	public string CategoryName { get; set; }

	public string Description { get; set; }

	public byte[] Picture { get; set; }

	public virtual ICollection<Product> Products
	{
		get { return _products; }
		set { _products = value; }
	}
}

public class Customer
{
	private ICollection<Order> _orders;

	public Customer()
	{
		_orders = new List<Order>();
	}

	public string CustomerID { get; set; }
	public string CompanyName { get; set; }
	public string ContactName { get; set; }
	public string ContactTitle { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
	public string Region { get; set; }
	public string PostalCode { get; set; }
	public string Country { get; set; }
	public string Phone { get; set; }
	public string Fax { get; set; }

	public virtual ICollection<Order> Orders
	{
		get { return _orders; }
		set { _orders = value; }
	}
}

public class Employee
{
	public Employee()
	{
		_subordinates = new List<Employee>();
		_orders = new List<Order>();
		_territories = new List<Territory>();
	}

	public int EmployeeID { get; set; }
	public string LastName { get; set; }
	public string FirstName { get; set; }
	public string Title { get; set; }
	public string TitleOfCourtesy { get; set; }
	public DateTime? BirthDate { get; set; }
	public DateTime? HireDate { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
	public string Region { get; set; }
	public string PostalCode { get; set; }
	public string Country { get; set; }
	public string HomePhone { get; set; }
	public string Extension { get; set; }
	public byte[] Photo { get; set; }
	public string Notes { get; set; }
	public int? ReportsTo { get; set; }
	public string PhotoPath { get; set; }
	public virtual Employee Manager { get; set; }

	private ICollection<Employee> _subordinates;
	public virtual ICollection<Employee> Subordinates
	{
		get { return _subordinates; }
		set { _subordinates = value; }
	}

	private ICollection<Order> _orders;
	public virtual ICollection<Order> Orders
	{
		get { return _orders; }
		set { _orders = value; }
	}

	private ICollection<Territory> _territories;
	public virtual ICollection<Territory> Territories
	{
		get { return _territories; }
		set { _territories = value; }
	}
}

public class Invoice
{
	public string ShipName { get; set; }
	public string ShipAddress { get; set; }
	public string ShipCity { get; set; }
	public string ShipRegion { get; set; }
	public string ShipPostalCode { get; set; }
	public string ShipCountry { get; set; }
	public string CustomerID { get; set; }
	public string CustomerName { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
	public string Region { get; set; }
	public string PostalCode { get; set; }
	public string Country { get; set; }
	public string Salesperson { get; set; }
	public int OrderID { get; set; }
	public DateTime? OrderDate { get; set; }
	public DateTime? RequiredDate { get; set; }
	public DateTime? ShippedDate { get; set; }
	public string ShipperName { get; set; }
	public int ProductID { get; set; }
	public string ProductName { get; set; }
	public decimal UnitPrice { get; set; }
	public short Quantity { get; set; }
	public float Discount { get; set; }
	public decimal? ExtendedPrice { get; set; }
	public decimal? Freight { get; set; }
}

public class Order
{
	public Order()
	{
		OrderDetails = new List<OrderDetail>();
	}

	public int OrderID { get; set; }
	public string CustomerID { get; set; }
	public int? EmployeeID { get; set; }
	public DateTime? OrderDate { get; set; }
	public DateTime? RequiredDate { get; set; }
	public DateTime? ShippedDate { get; set; }
	public int? ShipVia { get; set; }
	public decimal? Freight { get; set; }
	public string ShipName { get; set; }
	public string ShipAddress { get; set; }
	public string ShipCity { get; set; }
	public string ShipRegion { get; set; }
	public string ShipPostalCode { get; set; }
	public string ShipCountry { get; set; }
	public virtual Customer Customer { get; set; }
	public virtual Employee Employee { get; set; }
	public virtual ICollection<OrderDetail> OrderDetails { get; set; }
	public virtual Shipper Shipper { get; set; }
}

public class OrderDetail
{
	public int OrderID { get; set; }
	public int ProductID { get; set; }
	public decimal UnitPrice { get; set; }
	public short Quantity { get; set; }
	public float Discount { get; set; }
	public virtual Order Order { get; set; }
	public virtual Product Product { get; set; }
}

public class Product
{
	public Product()
	{
		_orderDetails = new List<OrderDetail>();
	}

	public int ProductID { get; set; }

	public string ProductName { get; set; }

	public int? SupplierID { get; set; }

	public int? CategoryID { get; set; }

	public string QuantityPerUnit { get; set; }

	public decimal? UnitPrice { get; set; }

	public short? UnitsInStock { get; set; }

	public short? UnitsOnOrder { get; set; }

	public short? ReorderLevel { get; set; }

	public bool Discontinued { get; set; }

	public virtual Category Category { get; set; }

	public virtual Supplier Supplier { get; set; }

	private ICollection<OrderDetail> _orderDetails;

	public virtual ICollection<OrderDetail> OrderDetails
	{
		get { return _orderDetails; }
		set { _orderDetails = value; }
	}
}

public class Region
{
	public Region()
	{
		Territories = new List<Territory>();
	}

	public int RegionID { get; set; }
	public string RegionDescription { get; set; }
	public virtual ICollection<Territory> Territories { get; set; }
}

public class Shipper
{
	public Shipper()
	{
		Orders = new List<Order>();
	}

	public int ShipperID { get; set; }
	public string CompanyName { get; set; }
	public string Phone { get; set; }
	public virtual ICollection<Order> Orders { get; set; }
}

public class Supplier
{
	public Supplier()
	{
		Products = new List<Product>();
	}

	public int SupplierID { get; set; }
	public string CompanyName { get; set; }
	public string ContactName { get; set; }
	public string ContactTitle { get; set; }
	public string Address { get; set; }
	public string City { get; set; }
	public string Region { get; set; }
	public string PostalCode { get; set; }
	public string Country { get; set; }
	public string Phone { get; set; }
	public string Fax { get; set; }
	public string HomePage { get; set; }
	public virtual ICollection<Product> Products { get; set; }
}

public class Territory
{
	public Territory()
	{
		Employees = new List<Employee>();
	}

	public string TerritoryID { get; set; }
	public string TerritoryDescription { get; set; }
	public int RegionID { get; set; }
	public virtual Region Region { get; set; }
	public virtual ICollection<Employee> Employees { get; set; }
}

#endregion

#region Mappings
public class CategoryMap : EntityTypeConfiguration<Category>
{
	public CategoryMap()
	{
		// Primary Key
		HasKey(t => t.CategoryID);

		// Properties
		Property(t => t.CategoryName)
			.IsRequired()
			.HasMaxLength(15);

		// Table & Column Mappings
		ToTable("Categories");
		Property(t => t.CategoryID).HasColumnName("CategoryID");
		Property(t => t.CategoryName).HasColumnName("CategoryName");
		Property(t => t.Description).HasColumnName("Description");
		Property(t => t.Picture).HasColumnName("Picture");
	}
}

public class CustomerMap : EntityTypeConfiguration<Customer>
{
	public CustomerMap()
	{
		// Primary Key
		HasKey(t => t.CustomerID);

		// Properties
		Property(t => t.CustomerID)
			.IsRequired()
			.IsFixedLength()
			.HasMaxLength(5);

		Property(t => t.CompanyName)
			.IsRequired()
			.HasMaxLength(40);

		Property(t => t.ContactName)
			.HasMaxLength(30);

		Property(t => t.ContactTitle)
			.HasMaxLength(30);

		Property(t => t.Address)
			.HasMaxLength(60);

		Property(t => t.City)
			.HasMaxLength(15);

		Property(t => t.Region)
			.HasMaxLength(15);

		Property(t => t.PostalCode)
			.HasMaxLength(10);

		Property(t => t.Country)
			.HasMaxLength(15);

		Property(t => t.Phone)
			.HasMaxLength(24);

		Property(t => t.Fax)
			.HasMaxLength(24);

		// Table & Column Mappings
		ToTable("Customers");
		Property(t => t.CustomerID).HasColumnName("CustomerID");
		Property(t => t.CompanyName).HasColumnName("CompanyName");
		Property(t => t.ContactName).HasColumnName("ContactName");
		Property(t => t.ContactTitle).HasColumnName("ContactTitle");
		Property(t => t.Address).HasColumnName("Address");
		Property(t => t.City).HasColumnName("City");
		Property(t => t.Region).HasColumnName("Region");
		Property(t => t.PostalCode).HasColumnName("PostalCode");
		Property(t => t.Country).HasColumnName("Country");
		Property(t => t.Phone).HasColumnName("Phone");
		Property(t => t.Fax).HasColumnName("Fax");
	}
}

public class EmployeeMap : EntityTypeConfiguration<Employee>
{
	public EmployeeMap()
	{
		// Primary Key
		HasKey(t => t.EmployeeID);

		// Properties
		Property(t => t.LastName)
			.IsRequired()
			.HasMaxLength(20);

		Property(t => t.FirstName)
			.IsRequired()
			.HasMaxLength(10);

		Property(t => t.Title)
			.HasMaxLength(30);

		Property(t => t.TitleOfCourtesy)
			.HasMaxLength(25);

		Property(t => t.Address)
			.HasMaxLength(60);

		Property(t => t.City)
			.HasMaxLength(15);

		Property(t => t.Region)
			.HasMaxLength(15);

		Property(t => t.PostalCode)
			.HasMaxLength(10);

		Property(t => t.Country)
			.HasMaxLength(15);

		Property(t => t.HomePhone)
			.HasMaxLength(24);

		Property(t => t.Extension)
			.HasMaxLength(4);

		Property(t => t.PhotoPath)
			.HasMaxLength(255);

		// Table & Column Mappings
		ToTable("Employees");
		Property(t => t.EmployeeID).HasColumnName("EmployeeID");
		Property(t => t.LastName).HasColumnName("LastName");
		Property(t => t.FirstName).HasColumnName("FirstName");
		Property(t => t.Title).HasColumnName("Title");
		Property(t => t.TitleOfCourtesy).HasColumnName("TitleOfCourtesy");
		Property(t => t.BirthDate).HasColumnName("BirthDate");
		Property(t => t.HireDate).HasColumnName("HireDate");
		Property(t => t.Address).HasColumnName("Address");
		Property(t => t.City).HasColumnName("City");
		Property(t => t.Region).HasColumnName("Region");
		Property(t => t.PostalCode).HasColumnName("PostalCode");
		Property(t => t.Country).HasColumnName("Country");
		Property(t => t.HomePhone).HasColumnName("HomePhone");
		Property(t => t.Extension).HasColumnName("Extension");
		Property(t => t.Photo).HasColumnName("Photo");
		Property(t => t.Notes).HasColumnName("Notes");
		Property(t => t.ReportsTo).HasColumnName("ReportsTo");
		Property(t => t.PhotoPath).HasColumnName("PhotoPath");

		// Relationships
		HasMany(t => t.Territories)
			.WithMany(t => t.Employees)
			.Map(m =>
					 {
						 m.ToTable("EmployeeTerritories");
						 m.MapLeftKey("EmployeeID");
						 m.MapRightKey("TerritoryID");
					 });

		HasOptional(t => t.Manager)
			.WithMany(t => t.Subordinates)
			.HasForeignKey(d => d.ReportsTo);
	}
}

public class InvoiceMap : EntityTypeConfiguration<Invoice>
{
	public InvoiceMap()
	{
		// Primary Key
		HasKey(
			t =>
			new
			{
				t.CustomerName,
				t.Salesperson,
				t.OrderID,
				t.ShipperName,
				t.ProductID,
				t.ProductName,
				t.UnitPrice,
				t.Quantity,
				t.Discount
			});

		// Properties
		Property(t => t.ShipName)
			.HasMaxLength(40);

		Property(t => t.ShipAddress)
			.HasMaxLength(60);

		Property(t => t.ShipCity)
			.HasMaxLength(15);

		Property(t => t.ShipRegion)
			.HasMaxLength(15);

		Property(t => t.ShipPostalCode)
			.HasMaxLength(10);

		Property(t => t.ShipCountry)
			.HasMaxLength(15);

		Property(t => t.CustomerID)
			.IsFixedLength()
			.HasMaxLength(5);

		Property(t => t.CustomerName)
			.IsRequired()
			.HasMaxLength(40);

		Property(t => t.Address)
			.HasMaxLength(60);

		Property(t => t.City)
			.HasMaxLength(15);

		Property(t => t.Region)
			.HasMaxLength(15);

		Property(t => t.PostalCode)
			.HasMaxLength(10);

		Property(t => t.Country)
			.HasMaxLength(15);

		Property(t => t.Salesperson)
			.IsRequired()
			.HasMaxLength(31);

		Property(t => t.OrderID)
			.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

		Property(t => t.ShipperName)
			.IsRequired()
			.HasMaxLength(40);

		Property(t => t.ProductID)
			.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

		Property(t => t.ProductName)
			.IsRequired()
			.HasMaxLength(40);

		Property(t => t.UnitPrice)
			.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

		Property(t => t.Quantity)
			.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

		// Table & Column Mappings
		ToTable("Invoices");
		Property(t => t.ShipName).HasColumnName("ShipName");
		Property(t => t.ShipAddress).HasColumnName("ShipAddress");
		Property(t => t.ShipCity).HasColumnName("ShipCity");
		Property(t => t.ShipRegion).HasColumnName("ShipRegion");
		Property(t => t.ShipPostalCode).HasColumnName("ShipPostalCode");
		Property(t => t.ShipCountry).HasColumnName("ShipCountry");
		Property(t => t.CustomerID).HasColumnName("CustomerID");
		Property(t => t.CustomerName).HasColumnName("CustomerName");
		Property(t => t.Address).HasColumnName("Address");
		Property(t => t.City).HasColumnName("City");
		Property(t => t.Region).HasColumnName("Region");
		Property(t => t.PostalCode).HasColumnName("PostalCode");
		Property(t => t.Country).HasColumnName("Country");
		Property(t => t.Salesperson).HasColumnName("Salesperson");
		Property(t => t.OrderID).HasColumnName("OrderID");
		Property(t => t.OrderDate).HasColumnName("OrderDate");
		Property(t => t.RequiredDate).HasColumnName("RequiredDate");
		Property(t => t.ShippedDate).HasColumnName("ShippedDate");
		Property(t => t.ShipperName).HasColumnName("ShipperName");
		Property(t => t.ProductID).HasColumnName("ProductID");
		Property(t => t.ProductName).HasColumnName("ProductName");
		Property(t => t.UnitPrice).HasColumnName("UnitPrice");
		Property(t => t.Quantity).HasColumnName("Quantity");
		Property(t => t.Discount).HasColumnName("Discount");
		Property(t => t.ExtendedPrice).HasColumnName("ExtendedPrice");
		Property(t => t.Freight).HasColumnName("Freight");
	}
}

public class OrderDetailMap : EntityTypeConfiguration<OrderDetail>
{
	public OrderDetailMap()
	{
		// Primary Key
		HasKey(t => new { t.OrderID, t.ProductID });

		// Properties
		Property(t => t.OrderID)
			.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

		Property(t => t.ProductID)
			.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

		// Table & Column Mappings
		ToTable("Order Details");
		Property(t => t.OrderID).HasColumnName("OrderID");
		Property(t => t.ProductID).HasColumnName("ProductID");
		Property(t => t.UnitPrice).HasColumnName("UnitPrice");
		Property(t => t.Quantity).HasColumnName("Quantity");
		Property(t => t.Discount).HasColumnName("Discount");

		// Relationships
		HasRequired(t => t.Order)
			.WithMany(t => t.OrderDetails)
			.HasForeignKey(d => d.OrderID);
		HasRequired(t => t.Product)
			.WithMany(t => t.OrderDetails)
			.HasForeignKey(d => d.ProductID);
	}
}

public class OrderMap : EntityTypeConfiguration<Order>
{
	public OrderMap()
	{
		// Primary Key
		HasKey(t => t.OrderID);

		// Properties
		Property(t => t.CustomerID)
			.IsFixedLength()
			.HasMaxLength(5);

		Property(t => t.ShipName)
			.HasMaxLength(40);

		Property(t => t.ShipAddress)
			.HasMaxLength(60);

		Property(t => t.ShipCity)
			.HasMaxLength(15);

		Property(t => t.ShipRegion)
			.HasMaxLength(15);

		Property(t => t.ShipPostalCode)
			.HasMaxLength(10);

		Property(t => t.ShipCountry)
			.HasMaxLength(15);

		// Table & Column Mappings
		ToTable("Orders");
		Property(t => t.OrderID).HasColumnName("OrderID");
		Property(t => t.CustomerID).HasColumnName("CustomerID");
		Property(t => t.EmployeeID).HasColumnName("EmployeeID");
		Property(t => t.OrderDate).HasColumnName("OrderDate");
		Property(t => t.RequiredDate).HasColumnName("RequiredDate");
		Property(t => t.ShippedDate).HasColumnName("ShippedDate");
		Property(t => t.ShipVia).HasColumnName("ShipVia");
		Property(t => t.Freight).HasColumnName("Freight");
		Property(t => t.ShipName).HasColumnName("ShipName");
		Property(t => t.ShipAddress).HasColumnName("ShipAddress");
		Property(t => t.ShipCity).HasColumnName("ShipCity");
		Property(t => t.ShipRegion).HasColumnName("ShipRegion");
		Property(t => t.ShipPostalCode).HasColumnName("ShipPostalCode");
		Property(t => t.ShipCountry).HasColumnName("ShipCountry");

		// Relationships
		HasOptional(t => t.Customer)
			.WithMany(t => t.Orders)
			.HasForeignKey(d => d.CustomerID);
		HasOptional(t => t.Employee)
			.WithMany(t => t.Orders)
			.HasForeignKey(d => d.EmployeeID);
		HasOptional(t => t.Shipper)
			.WithMany(t => t.Orders)
			.HasForeignKey(d => d.ShipVia);
	}
}

public class ProductMap : EntityTypeConfiguration<Product>
{
	public ProductMap()
	{
		// Primary Key
		HasKey(t => t.ProductID);

		// Properties
		Property(t => t.ProductName)
			.IsRequired()
			.HasMaxLength(40);

		Property(t => t.QuantityPerUnit)
			.HasMaxLength(20);

		// Table & Column Mappings
		ToTable("Products");
		Property(t => t.ProductID).HasColumnName("ProductID");
		Property(t => t.ProductName).HasColumnName("ProductName");
		Property(t => t.SupplierID).HasColumnName("SupplierID");
		Property(t => t.CategoryID).HasColumnName("CategoryID");
		Property(t => t.QuantityPerUnit).HasColumnName("QuantityPerUnit");
		Property(t => t.UnitPrice).HasColumnName("UnitPrice");
		Property(t => t.UnitsInStock).HasColumnName("UnitsInStock");
		Property(t => t.UnitsOnOrder).HasColumnName("UnitsOnOrder");
		Property(t => t.ReorderLevel).HasColumnName("ReorderLevel");
		Property(t => t.Discontinued).HasColumnName("Discontinued");

		// Relationships
		HasOptional(t => t.Category)
			.WithMany(t => t.Products)
			.HasForeignKey(d => d.CategoryID);
		HasOptional(t => t.Supplier)
			.WithMany(t => t.Products)
			.HasForeignKey(d => d.SupplierID);
	}
}

public class RegionMap : EntityTypeConfiguration<Region>
{
	public RegionMap()
	{
		// Primary Key
		HasKey(t => t.RegionID);

		// Properties
		Property(t => t.RegionID)
			.HasDatabaseGeneratedOption(DatabaseGeneratedOption.None);

		Property(t => t.RegionDescription)
			.IsRequired()
			.IsFixedLength()
			.HasMaxLength(50);

		// Table & Column Mappings
		ToTable("Region");
		Property(t => t.RegionID).HasColumnName("RegionID");
		Property(t => t.RegionDescription).HasColumnName("RegionDescription");
	}
}

public class ShipperMap : EntityTypeConfiguration<Shipper>
{
	public ShipperMap()
	{
		// Primary Key
		HasKey(t => t.ShipperID);

		// Properties
		Property(t => t.CompanyName)
			.IsRequired()
			.HasMaxLength(40);

		Property(t => t.Phone)
			.HasMaxLength(24);

		// Table & Column Mappings
		ToTable("Shippers");
		Property(t => t.ShipperID).HasColumnName("ShipperID");
		Property(t => t.CompanyName).HasColumnName("CompanyName");
		Property(t => t.Phone).HasColumnName("Phone");
	}
}

public class SupplierMap : EntityTypeConfiguration<Supplier>
{
	public SupplierMap()
	{
		// Primary Key
		HasKey(t => t.SupplierID);

		// Properties
		Property(t => t.CompanyName)
			.IsRequired()
			.HasMaxLength(40);

		Property(t => t.ContactName)
			.HasMaxLength(30);

		Property(t => t.ContactTitle)
			.HasMaxLength(30);

		Property(t => t.Address)
			.HasMaxLength(60);

		Property(t => t.City)
			.HasMaxLength(15);

		Property(t => t.Region)
			.HasMaxLength(15);

		Property(t => t.PostalCode)
			.HasMaxLength(10);

		Property(t => t.Country)
			.HasMaxLength(15);

		Property(t => t.Phone)
			.HasMaxLength(24);

		Property(t => t.Fax)
			.HasMaxLength(24);

		// Table & Column Mappings
		ToTable("Suppliers");
		Property(t => t.SupplierID).HasColumnName("SupplierID");
		Property(t => t.CompanyName).HasColumnName("CompanyName");
		Property(t => t.ContactName).HasColumnName("ContactName");
		Property(t => t.ContactTitle).HasColumnName("ContactTitle");
		Property(t => t.Address).HasColumnName("Address");
		Property(t => t.City).HasColumnName("City");
		Property(t => t.Region).HasColumnName("Region");
		Property(t => t.PostalCode).HasColumnName("PostalCode");
		Property(t => t.Country).HasColumnName("Country");
		Property(t => t.Phone).HasColumnName("Phone");
		Property(t => t.Fax).HasColumnName("Fax");
		Property(t => t.HomePage).HasColumnName("HomePage");
	}
}

public class TerritoryMap : EntityTypeConfiguration<Territory>
{
	public TerritoryMap()
	{
		// Primary Key
		HasKey(t => t.TerritoryID);

		// Properties
		Property(t => t.TerritoryID)
			.IsRequired()
			.HasMaxLength(20);

		Property(t => t.TerritoryDescription)
			.IsRequired()
			.IsFixedLength()
			.HasMaxLength(50);

		// Table & Column Mappings
		ToTable("Territories");
		Property(t => t.TerritoryID).HasColumnName("TerritoryID");
		Property(t => t.TerritoryDescription).HasColumnName("TerritoryDescription");
		Property(t => t.RegionID).HasColumnName("RegionID");

		// Relationships
		HasRequired(t => t.Region)
			.WithMany(t => t.Territories)
			.HasForeignKey(d => d.RegionID);
	}
}

#endregion
#endregion