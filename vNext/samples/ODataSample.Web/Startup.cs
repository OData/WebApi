using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using ODataSample.Web.Models;

namespace ODataSample.Web
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
            services.AddOData();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    builder =>
                    {
                        builder//.AllowAnyOrigin()
                               //.AllowAnyHeader()
                               .AllowAnyMethod()
                               .AllowCredentials();
                    });
            });

            services.AddSingleton<SampleContext>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseOData<ISampleService>("odata", builder =>
            {
                builder.Namespace = "Sample";
                builder.EntityType<Product>()
                    .Collection
                    .Function("MostExpensive")
                    .Returns<double>();
                builder.EntityType<Product>()
                    .Collection
                    .Function("MostExpensive2")
                    .Returns<double>();
                builder.EntityType<Product>()
                    .Function("ShortName")
                    .Returns<string>();
            });
            app.UseMvcWithDefaultRoute();
            //app.UseMvc(builder =>
            //{
            //    builder.MapODataRoute<ISampleService>("odata");
            //});
        }
        public static void Main(string[] args)
        {
            var application = new WebApplicationBuilder()
                 .UseConfiguration(WebApplicationConfiguration.GetDefault(args))
                 .UseStartup<Startup>()
                 .Build();

            application.Run();
        }
    }
}
