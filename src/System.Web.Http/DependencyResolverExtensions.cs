using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Metadata;
using System.Web.Http.ModelBinding;
using System.Web.Http.Properties;
using System.Web.Http.Services;
using System.Web.Http.Tracing;
using System.Web.Http.Validation;
using System.Web.Http.ValueProviders;

namespace System.Web.Http
{
    /// <summary>
    /// This provides a centralized list of type-safe accessors describing where and how we use the dependency resolver.
    /// This also provides a single entry point for each service request. That makes it easy 
    /// to see which parts of the code use it, and provides a single place to comment usage.
    /// Accessors encapsulate usage like:
    /// <list type="bullet">
    /// <item>Type-safe using {T} instead of unsafe <see cref="System.Type"/>.</item>
    /// <item>which type do we key off? This is interesting with type hierarchies.</item> 
    /// <item>do we ask for singular or plural?</item>
    /// <item>is it optional or mandatory?</item>
    /// <item>what are the ordering semantics</item>
    /// <item>Do we use a cached value or not?</item>     
    /// </list>
    /// Expected that any <see cref="IEnumerable{T}"/> we return is non-null, although possibly empty.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DependencyResolverExtensions
    {
        /// <summary>
        /// Get ValueProviderFactories. The order of returned providers is the priority order that we search the factories. 
        /// </summary>
        public static IEnumerable<ValueProviderFactory> GetValueProviderFactories(this DependencyResolver resolver)
        {
            return resolver.GetServices<ValueProviderFactory>();
        }

        /// <summary>
        /// Get a controller factory, which instantiates a string name into an <see cref="IHttpController"/>.
        /// This may be implemented by first getting the <see cref="Type"/> from the controller name, and 
        /// then using a <see cref="IHttpControllerActivator"/>.
        /// </summary>
        public static IHttpControllerFactory GetHttpControllerFactory(this DependencyResolver resolver)
        {
            return resolver.GetServiceOrThrow<IHttpControllerFactory>();
        }

        /// <summary>
        /// Controller activator is used to instantiate an <see cref="IHttpController"/>. 
        /// </summary>
        /// <returns>
        /// An <see cref="IHttpControllerActivator"/> instance or null if none are registered.
        /// </returns>
        public static IHttpControllerActivator GetHttpControllerActivator(this DependencyResolver resolver)
        {
            return resolver.GetServiceOrThrow<IHttpControllerActivator>();
        }

        public static IBuildManager GetBuildManager(this DependencyResolver resolver)
        {
            return resolver.GetServiceOrThrow<IBuildManager>();
        }

        public static IHttpActionSelector GetActionSelector(this DependencyResolver resolver)
        {
            return resolver.GetServiceOrThrow<IHttpActionSelector>();
        }

        public static IHttpActionInvoker GetActionInvoker(this DependencyResolver resolver)
        {
            return resolver.GetServiceOrThrow<IHttpActionInvoker>();
        }

        public static IApiExplorer GetApiExplorer(this DependencyResolver resolver)
        {
            return resolver.GetServiceOrThrow<IApiExplorer>();
        }

        public static IDocumentationProvider GetDocumentationProvider(this DependencyResolver resolver)
        {
            return resolver.GetService<IDocumentationProvider>();
        }

        public static IEnumerable<IFilterProvider> GetFilterProviders(this DependencyResolver resolver)
        {
            return resolver.GetServices<IFilterProvider>();
        }

        public static ModelMetadataProvider GetModelMetadataProvider(this DependencyResolver resolver)
        {
            // TODO: this is called a lot - should we use this.GetCachedService<T>? instead            
            return resolver.GetServiceOrThrow<ModelMetadataProvider>();
        }

        public static IEnumerable<ModelBinderProvider> GetModelBinderProviders(this DependencyResolver resolver)
        {
            return resolver.GetServices<ModelBinderProvider>();
        }

        public static IEnumerable<ModelValidatorProvider> GetModelValidatorProviders(this DependencyResolver resolver)
        {
            return resolver.GetServices<ModelValidatorProvider>();
        }

        public static IContentNegotiator GetContentNegotiator(this DependencyResolver resolver)
        {
            return resolver.GetService<IContentNegotiator>();
        }

        public static IActionValueBinder GetActionValueBinder(this DependencyResolver resolver)
        {
            return resolver.GetService<IActionValueBinder>();
        }

        public static ITraceManager GetTraceManager(this DependencyResolver resolver)
        {
            return resolver.GetService<ITraceManager>();
        }

        public static ITraceWriter GetTraceWriter(this DependencyResolver resolver)
        {
            return resolver.GetService<ITraceWriter>();
        }

        public static IBodyModelValidator GetBodyModelValidator(this DependencyResolver resolver)
        {
            return resolver.GetService<IBodyModelValidator>();
        }

        // Runtime code shouldn't call GetService() directly. Instead, have a wrapper (like the ones above) and call through the wrapper.
        private static TService GetService<TService>(this DependencyResolver resolver)
        {
            return (TService)resolver.GetService(typeof(TService));
        }

        private static IEnumerable<TService> GetServices<TService>(this DependencyResolver resolver)
        {
            return resolver.GetServices(typeof(TService)).Cast<TService>();
        }

        private static T GetServiceOrThrow<T>(this DependencyResolver resolver)
        {
            T result = resolver.GetService<T>();
            if (result == null)
            {
                throw Error.InvalidOperation(SRResources.DependencyResolverNoService, typeof(T).FullName);
            }

            return result;
        }
    }
}
