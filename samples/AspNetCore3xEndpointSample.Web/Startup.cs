//-----------------------------------------------------------------------------
// <copyright file="Startup.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using AspNetCore3xEndpointSample.Web.Models;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace AspNetCore3xEndpointSample.Web
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
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            IEdmModel model = EdmModelBuilder.GetEdmModel();

            // Please add "UseODataBatching()" before "UseRouting()" to support OData $batch.
            app.UseODataBatching();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapODataRoute(
                    "nullPrefix", null,
                    b =>
                    {
                        b.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp => model);
                        b.AddService<ODataDeserializerProvider>(Microsoft.OData.ServiceLifetime.Singleton, sp => new EntityReferenceODataDeserializerProvider(sp));
                        b.AddService<IEnumerable<IODataRoutingConvention>>(Microsoft.OData.ServiceLifetime.Singleton,
                            sp => ODataRoutingConventions.CreateDefaultWithAttributeRouting("nullPrefix", endpoints.ServiceProvider));
                    });

                endpoints.MapODataRoute("odataPrefix", "odata", model);

                endpoints.MapODataRoute("myPrefix", "my/{data}", model);

                endpoints.MapODataRoute("msPrefix", "ms", model, new DefaultODataBatchHandler());
            });
        }
    }

    public class EntityReferenceODataDeserializerProvider : DefaultODataDeserializerProvider
    {
        public EntityReferenceODataDeserializerProvider(IServiceProvider rootContainer)
            : base(rootContainer)
        {

        }

        public override ODataEdmTypeDeserializer GetEdmTypeDeserializer(IEdmTypeReference edmType)
        {
            return base.GetEdmTypeDeserializer(edmType);
        }

        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequest request)
        {
            return base.GetODataDeserializer(type, request);
        }
    }
}
