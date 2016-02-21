using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
            app.UseIISPlatformHandler();
            app.UseDeveloperExceptionPage();
            //app.UseMvc();
            //app.UseMvc(builder =>
            //{
            //    builder.MapODataRoute<ISampleService>("odata");
            //});
        }

        public static void Main(string[] args)
        {
            var application = new WebHostBuilder()
                .UseCaptureStartupErrors(true)
                .UseDefaultConfiguration(args)
                .UseIISPlatformHandlerUrl()
                .UseServer("Microsoft.AspNetCore.Server.Kestrel")
                .UseStartup<Startup>()
                .Build();

            application.Run();
        }
    }
}