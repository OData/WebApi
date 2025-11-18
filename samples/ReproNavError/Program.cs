using Microsoft.AspNet.OData.Extensions;

// This is a minimal repro for issue https://github.com/OData/WebApi/issues/2890

namespace ReproNavError
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddMvc(opts =>
            {
                opts.EnableEndpointRouting = false;
            });

            builder.Services.AddOData();

            var app = builder.Build();
            
            app.UseMvc(routeBuilder =>
            {
                routeBuilder.EnableDependencyInjection();
                routeBuilder.Select().Expand().Filter().OrderBy().MaxTop(100).Count();
                routeBuilder.MapODataServiceRoute("odata", "odata", ModelHelper.Model);
            });

            app.MapGet("/", () => "Hello World!");

            app.Run();
        }
    }
}
