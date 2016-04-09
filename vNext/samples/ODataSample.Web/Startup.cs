using System;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ODataSample.Web.Models;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace ODataSample.Web
{
    public class Startup
    {
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
			
            services.AddSingleton<ISampleService, ApplicationDbContext>();
		    services.AddOData<ISampleService>(builder =>
			{
				builder.Namespace = "Sample";
				builder
					.EntityType<Customer>();
				builder
					.EntityType<Product>()
					//.RemoveAllProperties()
					//.AddProperty(p => p.Name)
					//.AddProperty(p => p.Price)
					//.AddProperty(p =>)
					//.RemoveProperty(p => p.Price)
					;
				builder.EntityType<Product>()
					.HasKey(p => p.ProductId);
				builder.EntityType<ApplicationUser>()
					.RemoveAllProperties()
					.AddProperty(p => p.UserName)
					.AddProperty(p => p.Email);
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
			});
        }

		public void Configure(IApplicationBuilder app)
		{
			MigrateDatabase(app);
			Seeder.EnsureDatabase(app);
			//mvc.AddWebApiConventions();
			app.UseDeveloperExceptionPage();

			app.UseOData("odata");

			app.UseIISPlatformHandler();

			app.UseMvcWithDefaultRoute();
		}

		private static void MigrateDatabase(IApplicationBuilder app)
		{
			try
			{
				using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
					.CreateScope())
				{
					var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
					if (context != null)
					{
						context.Database.Migrate();
					}
					else
					{
						throw new Exception("Unable to resolve database context");
					}
				}
			}
			catch
			{
				throw;
			}
		}
	}
}