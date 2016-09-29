﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.AspNetCore.Routing;

namespace ODataSample.Web
{
    public class Startup
    {
        public Startup()
        {
        }

        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddOData();
            services.AddOData(options => options.RoutingConventions.Insert(0, new CustomRoutingConvention()));

            /* How to add customer's routing convention
            services.AddOData(
                options =>
                    options.RoutingConventions = new List<IODataRoutingConvention> {new CustomRoutingConvention()});
            */

            services.AddSingleton<SampleContext>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(LogLevel.Debug);

            IAssemblyProvider provider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            IEdmModel model = GetEdmModel(provider);
            IEdmModel model2 = GetPeopleEdmModel(provider);

            // Single
            app.UseMvc(builder => builder.MapODataRoute("odata", model));

            // Multiple: Option-1
            app.UseMvc(builder =>
            {
                builder.MapODataRoute("a", model);
                builder.MapODataRoute(model2);
            });

            // Multiple: Option-2
            app.UseMvc(builder => builder.MapODataRoute("odata1", model).MapODataRoute(model2));
        }

        private static IEdmModel GetEdmModel(IAssemblyProvider assemblyProvider)
        {
            var builder = new ODataConventionModelBuilder(assemblyProvider);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Product>("Products");

            // Functions
            var function = builder.EntityType<Customer>().Collection.Function("FindCustomersWithProductId");
            function.Parameter<int>("productId");
            function.ReturnsFromEntitySet<Customer>("Customers");


            function = builder.EntityType<Customer>().Function("GetCustomerName");
            function.Parameter<string>("format");
            function.Returns<string>();

            return builder.GetEdmModel();
        }

        private static IEdmModel GetPeopleEdmModel(IAssemblyProvider assemblyProvider)
        {
            var builder = new ODataConventionModelBuilder(assemblyProvider);
            builder.EntitySet<Person>("People");

            return builder.GetEdmModel();
        }
    }
}
