using System;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData.Builder;
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
            services.AddOData();
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
        }

		public void Configure(IApplicationBuilder app)
		{
			MigrateDatabase(app);
			Seeder.EnsureDatabase(app);
			//mvc.AddWebApiConventions();
			app.UseDeveloperExceptionPage();

			app.UseOData<ISampleService>("odata", builder =>
            {
                builder.Namespace = "Sample";
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
	            var printProductNameFunction =
		            builder
			            .EntityType<Product>()
			            .Function("PrintName")
			            .Returns<string>();
	            printProductNameFunction
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