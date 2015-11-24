using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
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
            services.AddMvcCore();
            services.AddOData();

            services.AddSingleton<SampleContext>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseOData<ISampleService>("odata");
            app.UseMvc(builder =>
            {
                builder.MapODataRoute<ISampleService>("odata");
            });
        }
    }
}
