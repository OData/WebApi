using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData;

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

            IAssemblyProvider provider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            IEdmModel model = GetEdmModel(provider);
            
            app.UseOData("odata", model);

            /*
            app.UseOData<ISampleService>("odata");
            app.UseMvc(builder =>
            {
                builder.MapODataRoute<ISampleService>("odata");
            });*/
        }

        private static IEdmModel GetEdmModel(IAssemblyProvider assemblyProvider)
        {
            var builder = new ODataConventionModelBuilder(assemblyProvider);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Product>("Products");
            return builder.GetEdmModel();
        }
    }
}
