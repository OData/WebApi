// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using AspNetCore3xODataSample.Web.Models;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ServiceLifetime = Microsoft.OData.ServiceLifetime;

namespace AspNetCore3xODataSample.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CustomerOrderContext>(opt => opt.UseLazyLoadingProxies().UseInMemoryDatabase("CustomerOrderList"));
            services.AddOData();
            // services.AddMvc(options => options.EnableEndpointRouting = false);
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            IEdmModel model = EdmModelBuilder.GetEdmModel();

            /*
            app.UseMvc(builder =>
            {
                builder.Select().Expand().Filter().OrderBy().MaxTop(100).Count();

                builder.MapODataServiceRoute("odata", "odata", model);
            });*/

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapODataServiceRoute("odata", "odata", model);

                endpoints.MapODataServiceRoute("odata2", "myprefix", model);

            });
        }
    }
}
