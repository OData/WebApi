using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ODataSample.Web.Models;

namespace ODataSample.Web
{
    public class Startup
    {
	    public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
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

            services.AddSingleton<SampleContext>();
        }

		public void Configure(IApplicationBuilder app)
		{
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
	}
}