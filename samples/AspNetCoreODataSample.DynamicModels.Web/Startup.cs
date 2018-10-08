// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using AspNetCoreODataSample.DynamicModels.Web.Edm;
using AspNetCoreODataSample.DynamicModels.Web.Models;
using AspNetCoreODataSample.DynamicModels.Web.Utils;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Routing.Conventions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace AspNetCoreODataSample.DynamicModels.Web
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
            services.AddDbContext<HouseContext>(opt => opt.UseInMemoryDatabase("HouseList"));
            services.AddSingleton<IPluralizer, SimplePluralizer>();

            // Register model builder as scoped to let it use the per-request DbContext
            services.AddScoped<EdmModelBuilder>();

            services.AddOData();
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            using (var serviceScope = app.ApplicationServices.CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<HouseContext>();
                AddTestData(context);
            }


            app.UseMvc(builder =>
            {
                builder.Select().Expand().Filter().OrderBy().MaxTop(100).Count();

                builder.MapODataServiceRoute("odata", "api", odataBuilder =>
                {
                    // add the IEdmModel as scoped service always have up-to-date models provided and used. 
                    // the EdmModelBuilder internally should to a caching. 
                    odataBuilder.AddService(Microsoft.OData.ServiceLifetime.Scoped, sp =>
                        // EdmModelBuilder is registered in the asp.net core application, not in the odata specific DI
                        // therefore we need to dig down to the correct service provider 
                        sp.GetService<HttpRequestScope>().HttpRequest.HttpContext.RequestServices.GetService<EdmModelBuilder>().GetEdmModel());

                    // The routing conventions which will redirect to the generic InteriorController for accessing entity sets
                    odataBuilder.AddService(Microsoft.OData.ServiceLifetime.Singleton, sp =>
                    {
                        IList<IODataRoutingConvention> routingConventions = ODataRoutingConventions.CreateDefaultWithAttributeRouting("odata", builder);
                        routingConventions.Add(new InteriorRoutingConvention());
                        return routingConventions.ToList().AsEnumerable();
                    });
                });
            });
        }

        private void AddTestData(HouseContext context)
        {
            // NOTE: save changes inbetween ensure we have IDs for our entities

            // Add interior definitions for chairs and tables:

            // class Chair
            // {
            //     public string Manufacturer {get;set;}
            //     public string Model {get;set;}
            //     public int Year {get;set;}
            //     public double Weight {get;set;}
            // }

            var chairDefinition = new InteriorDefinition
            {
                Name = "Chair",
            };
            context.Add(chairDefinition);
            context.SaveChanges();

            context.Add(new InteriorPropertyDefinition
            {
                Name = "Manufacturer",
                PropertyType = InteriorPropertyType.String,
                PropertyName = nameof(IUserDefinedPropertyBag.StringProperty1),
                DefinitionID = chairDefinition.ID,
                Definition = chairDefinition,
            });
            context.Add(new InteriorPropertyDefinition
            {
                Name = "Model",
                PropertyType = InteriorPropertyType.String,
                PropertyName = nameof(IUserDefinedPropertyBag.StringProperty2),
                DefinitionID = chairDefinition.ID,
                Definition = chairDefinition,
            });
            context.Add(new InteriorPropertyDefinition
            {
                Name = "Year",
                PropertyType = InteriorPropertyType.Int,
                PropertyName = nameof(IUserDefinedPropertyBag.IntProperty1),
                DefinitionID = chairDefinition.ID,
                Definition = chairDefinition,
            });

            context.Add(new InteriorPropertyDefinition
            {
                Name = "Weight",
                PropertyType = InteriorPropertyType.Double,
                PropertyName = nameof(IUserDefinedPropertyBag.DoubleProperty1),
                DefinitionID = chairDefinition.ID,
                Definition = chairDefinition,
            });
            context.SaveChanges();


            // class Table
            // {
            //     public string Manufacturer {get;set;}
            //     public string Model {get;set;}
            //     public int ExpansionPanels {get;set;}
            //     public int SuitablePersonCount {get;set;}
            //     public double Width {get;set;}
            //     public double Height {get;set;}
            //     public double Depth {get;set;}
            // }

            var tableDefinition = new InteriorDefinition
            {
                Name = "Table",
            };
            context.Add(tableDefinition);
            context.SaveChanges();

            context.Add(new InteriorPropertyDefinition
            {
                Name = "Manufacturer",
                PropertyType = InteriorPropertyType.String,
                PropertyName = nameof(IUserDefinedPropertyBag.StringProperty1),
                DefinitionID = tableDefinition.ID,
                Definition = tableDefinition,
            });
            context.Add(new InteriorPropertyDefinition
            {
                Name = "Model",
                PropertyType = InteriorPropertyType.String,
                PropertyName = nameof(IUserDefinedPropertyBag.StringProperty2),
                DefinitionID = tableDefinition.ID,
                Definition = tableDefinition,
            });
            context.Add(new InteriorPropertyDefinition
            {
                Name = "ExpansionPanels",
                PropertyType = InteriorPropertyType.Int,
                PropertyName = nameof(IUserDefinedPropertyBag.IntProperty1),
                DefinitionID = tableDefinition.ID,
                Definition = tableDefinition,
            });
            context.Add(new InteriorPropertyDefinition
            {
                Name = "SuitablePersonCount",
                PropertyType = InteriorPropertyType.Int,
                PropertyName = nameof(IUserDefinedPropertyBag.IntProperty2),
                DefinitionID = tableDefinition.ID,
                Definition = tableDefinition,
            });
            context.Add(new InteriorPropertyDefinition
            {
                Name = "Width",
                PropertyType = InteriorPropertyType.Double,
                PropertyName = nameof(IUserDefinedPropertyBag.DoubleProperty1),
                DefinitionID = tableDefinition.ID,
                Definition = tableDefinition,
            });
            context.Add(new InteriorPropertyDefinition
            {
                Name = "Height",
                PropertyType = InteriorPropertyType.Double,
                PropertyName = nameof(IUserDefinedPropertyBag.DoubleProperty2),
                DefinitionID = tableDefinition.ID,
                Definition = tableDefinition,
            });
            context.Add(new InteriorPropertyDefinition
            {
                Name = "Depth",
                PropertyType = InteriorPropertyType.Double,
                PropertyName = nameof(IUserDefinedPropertyBag.DoubleProperty3),
                DefinitionID = tableDefinition.ID,
                Definition = tableDefinition,
            });
            context.SaveChanges();

            // Add some test houses and rooms (random data)

            var deskManufacturers = new[] { "Desk Inc.", "Desktopia", "Desk4You", "WeLoveDesks" };
            var deskModels = new[] { "Dining Table", "Couch Table", "Pool Table", "Table Football Table", "Poker Table" };

            var chairManufacturers = new[] { "Chair Inc.", "Chairtopia", "Chair4You", "WeLoveChairs" };
            var chairModels = new[] { "Armchair", "Rocking Chair", "Stool", "Wheelchair", "Deckchair" };

            var random = new Random();
            for (int houseIndex = 0; houseIndex < random.Next(1, 5); houseIndex++)
            {
                var house = new House
                {
                    Name = "House " + houseIndex,
                    Address = "Main Street " + random.Next(1, 100)
                };
                context.Add(house);
                context.SaveChanges();

                for (int roomIndex = 0; roomIndex < random.Next(1, 10); roomIndex++)
                {
                    var room = new Room
                    {
                        Name = house.Name + " - Room " + roomIndex,
                        House = house,
                        HouseID = house.ID
                    };
                    context.Add(room);
                    context.SaveChanges();

                    for (int tableIndex = 0; tableIndex < random.Next(1, 5); tableIndex++)
                    {
                        var table = new Interior
                        {
                            Definition = tableDefinition,
                            DefinitionID = tableDefinition.ID,

                            StringProperty1 = deskManufacturers[random.Next(deskManufacturers.Length)],
                            StringProperty2 = deskModels[random.Next(deskModels.Length)],
                            IntProperty1 = random.Next(0, 2), // ExpansionPanels
                            IntProperty2 = random.Next(4, 10), // SuitablePersonCount
                            DoubleProperty1 = random.NextDouble() * 300, // Width (0-300cm)
                            DoubleProperty2 = random.NextDouble() * 120, // Height (0-120cm)
                            DoubleProperty3 = random.NextDouble() * 800, // Depth (0-800cm)
                        };
                        context.Add(table);
                    }
                    context.SaveChanges();

                    for (int chairIndex = 0; chairIndex < random.Next(1, 10); chairIndex++)
                    {
                        var chair = new Interior
                        {
                            Definition = chairDefinition,
                            DefinitionID = chairDefinition.ID,

                            StringProperty1 = chairManufacturers[random.Next(deskManufacturers.Length)],
                            StringProperty2 = chairModels[random.Next(deskModels.Length)],
                            IntProperty1 = random.Next(2000, DateTime.Now.Year + 1), // Year
                            DoubleProperty1 = 500 + random.NextDouble() * 500, // Weight (500-1000g)
                        };
                        context.Add(chair);
                    }
                    context.SaveChanges();
                }
            }
        }
    }
}
