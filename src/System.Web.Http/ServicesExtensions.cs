// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.Dispatcher;
using System.Web.Http.Filters;
using System.Web.Http.Hosting;
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
    /// This provides a centralized list of type-safe accessors describing where and how we get services.
    /// This also provides a single entry point for each service request. That makes it easy
    /// to see which parts of the code use it, and provides a single place to comment usage.
    /// Accessors encapsulate usage like:
    /// <list type="bullet">
    /// <item>Type-safe using {T} instead of unsafe <see cref="System.Type"/>.</item>
    /// <item>which type do we key off? This is interesting with type hierarchies.</item>
    /// <item>do we ask for singular or plural?</item>
    /// <item>is it optional or mandatory?</item>
    /// <item>what are the ordering semantics</item>
    /// </list>
    /// Services that are per-controller hang off <see cref="ControllerServices"/>, 
    /// whereas global services hang off <see cref="DefaultServices"/>
    /// Expected that any <see cref="IEnumerable{T}"/> we return is non-null, although possibly empty.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServicesExtensions
    {
        public static IEnumerable<ModelBinderProvider> GetModelBinderProviders(this ServicesContainer services)
        {
            return services.GetServices<ModelBinderProvider>();
        }
                
        public static ModelMetadataProvider GetModelMetadataProvider(this ServicesContainer services)
        {
            return services.GetServiceOrThrow<ModelMetadataProvider>();
        }
                
        public static IEnumerable<ModelValidatorProvider> GetModelValidatorProviders(this ServicesContainer services)
        {
            return services.GetServices<ModelValidatorProvider>();
        }
                
        public static IContentNegotiator GetContentNegotiator(this ServicesContainer services)
        {
            return services.GetService<IContentNegotiator>();
        }

        /// <summary>
        /// Controller activator is used to instantiate an <see cref="IHttpController"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="IHttpControllerActivator"/> instance or null if none are registered.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IHttpControllerActivator GetHttpControllerActivator(this ControllerServices services)
        {
            return services.GetServiceOrThrow<IHttpControllerActivator>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IHttpActionSelector GetActionSelector(this ControllerServices services)
        {
            return services.GetServiceOrThrow<IHttpActionSelector>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IHttpActionInvoker GetActionInvoker(this ControllerServices services)
        {
            return services.GetServiceOrThrow<IHttpActionInvoker>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IActionValueBinder GetActionValueBinder(this ControllerServices services)
        {
            return services.GetService<IActionValueBinder>();
        }

        /// <summary>
        /// Get ValueProviderFactories. The order of returned providers is the priority order that we search the factories.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IEnumerable<ValueProviderFactory> GetValueProviderFactories(this ControllerServices services)
        {
            return services.GetServices<ValueProviderFactory>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IBodyModelValidator GetBodyModelValidator(this ControllerServices services)
        {
            return services.GetService<IBodyModelValidator>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IHostBufferPolicySelector GetHostBufferPolicySelector(this DefaultServices services)
        {
            return services.GetService<IHostBufferPolicySelector>();
        }

        /// <summary>
        /// Get a controller selector, which selects an <see cref="HttpControllerDescriptor"/> given an <see cref="HttpRequestMessage"/>.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IHttpControllerSelector GetHttpControllerSelector(this DefaultServices services)
        {
            return services.GetServiceOrThrow<IHttpControllerSelector>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IAssembliesResolver GetAssembliesResolver(this DefaultServices services)
        {
            return services.GetServiceOrThrow<IAssembliesResolver>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IHttpControllerTypeResolver GetHttpControllerTypeResolver(this DefaultServices services)
        {
            return services.GetServiceOrThrow<IHttpControllerTypeResolver>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IApiExplorer GetApiExplorer(this DefaultServices services)
        {
            return services.GetServiceOrThrow<IApiExplorer>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IDocumentationProvider GetDocumentationProvider(this DefaultServices services)
        {
            return services.GetService<IDocumentationProvider>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static IEnumerable<IFilterProvider> GetFilterProviders(this DefaultServices services)
        {
            return services.GetServices<IFilterProvider>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static ITraceManager GetTraceManager(this DefaultServices services)
        {
            return services.GetService<ITraceManager>();
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification = "derived type determines global vs. controller service")]
        public static ITraceWriter GetTraceWriter(this DefaultServices services)
        {
            return services.GetService<ITraceWriter>();
        }

        // Runtime code shouldn't call GetService() directly. Instead, have a wrapper (like the ones above) and call through the wrapper.
        private static TService GetService<TService>(this ServicesContainer services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            return (TService)services.GetService(typeof(TService));
        }

        private static IEnumerable<TService> GetServices<TService>(this ServicesContainer services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            return services.GetServices(typeof(TService)).Cast<TService>();
        }

        private static T GetServiceOrThrow<T>(this ServicesContainer services)
        {
            T result = services.GetService<T>();
            if (result == null)
            {
                throw Error.InvalidOperation(SRResources.DependencyResolverNoService, typeof(T).FullName);
            }

            return result;
        }
    }
}
