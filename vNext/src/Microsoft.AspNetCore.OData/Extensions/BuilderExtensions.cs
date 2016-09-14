using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Extensions
{
    public static class BuilderExtensions
    {
        public static IRouteBuilder MapODataRoute(this IRouteBuilder builder, string prefix, IEdmModel model)
        {
            IRouter target = builder.ServiceProvider.GetRequiredService<MvcRouteHandler>();

            var inlineConstraintResolver = builder
                .ServiceProvider
                .GetRequiredService<IInlineConstraintResolver>();

            ODataRouteConstraint constraint = new ODataRouteConstraint(prefix, model);
            builder.Routes.Add(new ODataRoute(target, prefix, constraint, inlineConstraintResolver));
            return builder;
        }

        public static IApplicationBuilder UseOData([NotNull] this IApplicationBuilder app, string prefix, IEdmModel model)
        {
            var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            AssemblyProviderManager.Register(defaultAssemblyProvider);

            IRouter target = app.ApplicationServices.GetRequiredService<MvcRouteHandler>();

            var inlineConstraintResolver = app
                .ApplicationServices
                .GetRequiredService<IInlineConstraintResolver>();

            ODataRouteConstraint constraint = new ODataRouteConstraint(prefix, model);

            ODataRoute route = new ODataRoute(target, prefix, constraint, inlineConstraintResolver);

            var routes = new RouteBuilder(app)
            {
                DefaultHandler = app.ApplicationServices.GetRequiredService<MvcRouteHandler>()
            };
            routes.Routes.Add(route);

            return app.UseRouter(routes.Build());

            // return app.UseMvc(routeBuilder => routeBuilder.Routes.Add(route));

            // return app.UseRouter(new ODataRoute(target, prefix, constraint, inlineConstraintResolver));
        }

        /*
        public static IRouteBuilder MapODataRoute<T>(this IRouteBuilder builder, string prefix) where T : class
        {
            builder.Routes.Add(new ODataRoute(prefix, DefaultODataModelProvider.BuildEdmModel(typeof(T))));
            return builder;
        }

        public static IApplicationBuilder UseOData<T>([NotNull] this IApplicationBuilder app, string prefix) where T : class
        {
            var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            AssemblyProviderManager.Register(defaultAssemblyProvider);

            return app.UseRouter(new ODataRoute(prefix, DefaultODataModelProvider.BuildEdmModel(typeof(T))));
        }
        */

        public static IApplicationBuilder InitializeODataBuilder([NotNull] this IApplicationBuilder app)        {
            var defaultAssemblyProvider = app.ApplicationServices.GetRequiredService<IAssemblyProvider>();
            AssemblyProviderManager.Register(defaultAssemblyProvider);

            return app;
        }
    }
}
