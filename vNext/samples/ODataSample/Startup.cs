using System;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ODataSample.Web.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.PlatformAbstractions;

namespace ODataSample.Web
{
	public abstract class StartupBase
	{
		public static void Init<TStartup>(Action<IWebHostBuilder> building = null, params string[] args) where TStartup : class
		{
			var host = new WebHostBuilder()
				.CaptureStartupErrors(true)
//				.ConfigureLogging(options => options.AddDebug())
				.UseDefaultHostingConfiguration(args);
			building?.Invoke(host);
			//.UseIISIntegration()
			host.UseKestrel()
				.UseStartup<TStartup>();
			host.Build().Run();
		}

		protected StartupBase(
			IHostingEnvironment env,
			IRuntimeEnvironment runtimeEnvironment,
			string environment = null)
		{
			// Set up configuration sources.
			var builder = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{environment}.json", true)
				;


			//if (env.IsDevelopment())
			//{
			//	// For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
			//	builder.AddUserSecrets();

			//	//// This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
			//	//builder.AddApplicationInsightsSettings(developerMode: true);
			//}

			builder.AddEnvironmentVariables();
			builder.Build();
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddEntityFramework()
				.AddDbContext<ApplicationDbContext>();
			services.AddEntityFrameworkSqlServer();
			services.AddMvc()
				.AddWebApiConventions();
			services.AddMvcDnx();

			services.AddCors(options =>
			{
				options.AddPolicy("AllowAll",
					builder =>
					{
						builder //.AllowAnyOrigin()
								//.AllowAnyHeader()
							.AllowAnyMethod()
							.AllowCredentials();
					});
			});
			services.AddIdentity<ApplicationUser, IdentityRole>(options =>
			{
				// Control password strength requirements here
				options.Password.RequireDigit = true;
				//options.Tokens.
			})
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();


			services.AddSingleton<ISampleService, ApplicationDbContext>();
			//services.ConfigureODataOutputFormatter<SampleOutputFormatter>();
			services.ConfigureODataSerializerProvider<SampleODataSerializerProvider>();
			services.AddOData<ISampleService>(builder =>
			{
				// OData actions are HTTP POST
				// OData functions are HTTP GET

				builder.Namespace = "Sample";
				//builder.AddSerializeInterceptor<string>(interceptor =>
				//{
				//	if (interceptor.DeclaringType == typeof(Order))
				//	{
				//		interceptor.Value = (interceptor.Value ?? "") + "!";
				//	}
				//	return true;
				//});
				builder.EntityType<Order>()
					.Property(p => p.Title)
					//.UseSerializer(value =>
					//{
					//	value.Value = "Hey";
					//	return true;
					//})
					;
				builder.EntityType<ApplicationUser>()
					.RemoveAllProperties()
					//.AddProperty(p => p.Roles)
					.AddProperty(p => p.UserName)
					.AddProperty(p => p.Email)
					.AddProperty(p => p.FavouriteProductId)
					.AddProperty(p => p.FavouriteProduct)
					;
				builder
					.EntityType<Order>()
					.HasKey(o => o.Id);
				builder
					.EntityType<Customer>()
					.Property(p => p.CustomerId)
					;
				builder
					.EntityType<Product>()
					;
				//builder.EntityType<Product>()
				//	.HasKey(p => p.ProductId);
				builder.EntityType<ApplicationUser>()
					.HasKey(p => p.Id);
				builder
					.Function("HelloWorld")
					.Returns<string>();
				builder
					.Function("HelloComplexWorld")
					.Returns<Permissions>();
				var multiplyFunction = builder
					.Function("Multiply");
				multiplyFunction
					.Parameter<float>("a");
				multiplyFunction
					.Parameter<float>("b");
				multiplyFunction
					.Returns<float>();
				builder
					.EntityType<Product>()
					.Collection
					.Function("MostExpensive")
					.Returns<double>();
				var getProductNameFunction =
					builder
						.EntityType<Product>()
						.Function("GetName")
						.Returns<string>();
				getProductNameFunction
					.Parameter<string>("prefix");
				var postProductNameFunction =
					builder
						.EntityType<Product>()
						.Action("PostName")
						.Returns<string>();
				postProductNameFunction
					.Parameter<string>("prefix");
				builder
					.EntityType<Product>()
					.Collection
					.Function("MostExpensive2")
					.Returns<double>();
				builder
					.EntityType<Product>()
					.Function("ShortName")
					.Returns<string>();
				var validateField =
				builder
					.Action("ValidateField")
					.Returns<string>();
				validateField.Parameter<string>("SetName");
				validateField.Parameter<string>("Name");
				validateField.Parameter<string>("Value");
				//builder
				//	.EntityType<Product>()
				//	.Collection
				//	.Action("ValidateField")
				//	;
				//builder
				//	.EntityType<Product>()
				//	.Collection
				//	.Action("ValidateField")
				//	;
				//validateField.Parameter<string>("Name");
				//validateField.Parameter<string>("Value");

				builder
					.EntityType<Order>()
					.Function("DuplicateMethodName")
					.Returns<string>();
				builder
					.EntityType<Product>()
					.Function("DuplicateMethodName")
					.Returns<string>();
			});
		}

		public void Configure(IApplicationBuilder app,
			UserManager<ApplicationUser> userManager,
			RoleManager<IdentityRole> roleManager
			)
		{
			Seeder.MigrateDatabaseAsync(app.ApplicationServices);

			app.UseDeveloperExceptionPage();

			app.UseIISPlatformHandler();

			app.UseIdentity();

			app.UseOData("odata");

			app.UseStaticFiles();

			var defaultFilesOptions = new DefaultFilesOptions();
			defaultFilesOptions.DefaultFileNames.Add("index.html");
			app.UseDefaultFiles(defaultFilesOptions);

			app.UseMvc(
				routes =>
				{
					routes.MapRoute("default", "{controller=Home}/{action=Index}/{id?}");
					//appSettings.MvcRouter = routes.Build();
				});

			//app.UseMvcWithDefaultRoute();
		}
	}
}