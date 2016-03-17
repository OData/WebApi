using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.OData.Extensions;
using ODataSample.Web.Models;

namespace ODataSample.Web
{
    public class Startup
    {
        public Startup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOData();

            services.AddSingleton<SampleContext>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);

            app.UseOData<ISampleService>("odata");
            app.UseMvc(builder =>
            {
                builder.MapODataRoute<ISampleService>("odata");
            });
        }
    }
}
