using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using ODataSample.Web.Models;

namespace OData.Sample
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
			ODataModelBuilder builder = new ODataConventionModelBuilder();
			builder.EntitySet<Product>("Products");
			builder.EntitySet<Customer>("Customers");
			builder.EntitySet<ApplicationUser>("Users");

			config.MapODataServiceRoute(
				routeName: "ODataRoute",
				routePrefix: null,
				model: builder.GetEdmModel());
			// Web API configuration and services
														  // Configure Web API to use only bearer token authentication.
			//config.SuppressDefaultHostAuthentication();
            //config.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
			Seeder.EnsureDatabase(new ApplicationDbContext());
        }
    }
}
