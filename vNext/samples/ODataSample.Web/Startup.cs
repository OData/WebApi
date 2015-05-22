using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.OData;
using Microsoft.Framework.DependencyInjection;
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

            services.AddSingleton<SampleContext>();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMvc(builder => {
                builder.MapODataRoute<ISampleService>("odata");
            });
        }
    }
}
