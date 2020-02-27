// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using AspNetCore3xEndpointSample.Web.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OData.Edm;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Batch;
using Microsoft.AspNet.OData.Formatter.Deserialization;
using System;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.OData;
using System.Collections.Generic;
using Microsoft.AspNet.OData.Routing.Conventions;

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

            app.UseRouting();

            app.UseODataBatching();

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
