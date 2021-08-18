//-----------------------------------------------------------------------------
// <copyright file="ODataConventionModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// <see cref="ODataConventionModelBuilder"/> is used to automatically map CLR classes to an EDM model based on a set of.
    /// </summary>
    public partial class ODataConventionModelBuilder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// This constructor will work stand-alone scenarios and require using the
        /// <see cref="AppDomain"/> to get a list of assemblies in the domain.
        /// </summary>
        public ODataConventionModelBuilder()
            : this(WebApiAssembliesResolver.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// This constructor uses the <see cref="ApplicationPartManager"/> from AspNetCore obtained
        /// from the <see cref="IServiceProvider"/> to get a list of assemblies for modeling.
        /// </summary>
        /// <param name="provider">The service provider to use.</param>
        public ODataConventionModelBuilder(IServiceProvider provider)
            : this(provider, null, isQueryCompositionMode: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// This constructor uses the <see cref="ApplicationPartManager"/> from AspNetCore
        ///  to get a list of assemblies for modeling.
        /// </summary>
        /// <param name="applicationPartManager">The application part manager to use.</param>
        /// <remarks> 
        /// This function uses types that are AspNetCore-specific.
        /// </remarks>
        public ODataConventionModelBuilder(ApplicationPartManager applicationPartManager)
            : this(null, applicationPartManager, isQueryCompositionMode: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// This constructor uses the <see cref="ApplicationPartManager"/> from AspNetCore obtained
        /// from the <see cref="IServiceProvider"/> to get a list of assemblies for modeling.
        /// The model built if <paramref name="isQueryCompositionMode"/> is <c>true</c> has more relaxed
        /// inference rules and also treats all types as entity types. This constructor is intended
        /// for use by unit testing only.
        /// </summary>
        /// <param name="provider">The service provider to use.</param>
        /// <param name="isQueryCompositionMode">If the model is being built for only querying.</param>
        public ODataConventionModelBuilder(IServiceProvider provider, bool isQueryCompositionMode)
            : this(provider, null, isQueryCompositionMode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// The model built if <paramref name="isQueryCompositionMode"/> is <c>true</c> has more relaxed
        /// inference rules and also treats all types as entity types.
        /// </summary>
        /// <param name="provider">The service provider to use.</param>
        /// <param name="applicationPartManager">
        /// The application part manager to use. If null, the service
        /// provider will be queried for the application part manager.
        /// </param>
        /// <param name="isQueryCompositionMode">If the model is being built for only querying.</param>
        private ODataConventionModelBuilder(
            IServiceProvider provider,
            ApplicationPartManager applicationPartManager,
            bool isQueryCompositionMode)
        {

            // Create an IWebApiAssembliesResolver from configuration and initialize.
            if (applicationPartManager == null)
            {
                if (provider == null)
                {
                    throw Error.ArgumentNull("provider");
                }

                applicationPartManager = provider.GetRequiredService<ApplicationPartManager>();
            }

            IWebApiAssembliesResolver internalResolver = new WebApiAssembliesResolver(applicationPartManager);
            Initialize(internalResolver, isQueryCompositionMode);
        }
    }
}
