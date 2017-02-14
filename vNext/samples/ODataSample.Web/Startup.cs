// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Builder;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OData.Edm;
using ODataSample.Web.Models;

namespace ODataSample.Web
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOData();
            //services.AddOData(options =>
            //{
            //    //options.RoutingConventions.Insert(0, new AttributeRoutingConvention());
            //   // options.RoutingConventions.Insert(0, new DefaultODataRoutingConvention());
            //    options.RoutingConventions.Insert(0, new MetadataRoutingConvention());
            //});

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

            var provider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            var model = GetEdmModel(provider);
            var model2 = GetPeopleEdmModel(provider);

            //// Single
            //app.UseMvc(builder => builder.MapODataRoute("odata", model));

            //// Multiple: Option-1
            //app.UseMvc(builder =>
            //{
            //    builder.MapODataRoute("a", model);
            //    builder.MapODataRoute(model2);
            //});

            // Multiple: Option-2
            app.UseMvc(builder => builder.MapODataRoute("odata", model));
        }

        private static IEdmModel GetEdmModel(IAssemblyProvider assemblyProvider)
        {
            var builder = new ODataConventionModelBuilder(assemblyProvider);
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<Product>("Products");
            builder.EntityType<Customer>().HasKey(x => x.CustomerId).Property(x => x.FirstName);

            var action = builder.EntityType<Customer>().Collection.Action("Ping");
            action.Parameter<int>("customerId");
            action.Returns<bool>();

            // Functions
            var fun = builder.EntityType<Customer>().Collection.Function("Pong");
            fun.Parameter<int>("customerId");
            fun.Returns<bool>();


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