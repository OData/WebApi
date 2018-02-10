// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
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
        /// </summary>
        /// <remarks>
        /// This constructor will work stand-alone scenarios but it does require using the
        /// <see cref="AppDomain"/> to get a list of assemblies in the domain to build
        /// the model. In contrast, this constructor will not work in ASP.NET Core 1.x
        /// due to the lack of AppDomain.
        /// </remarks>
        private ODataConventionModelBuilder()
            : this(WebApiAssembliesResolver.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="provider">The service provider to use.</param>
        /// <remarks>
        /// While this function does not use types that are AspNetCore-specific,
        /// the functionality is due to the way assembly resolution is done in AspNet vs AspnetCore.
        /// </remarks>
        public ODataConventionModelBuilder(IServiceProvider provider)
            : this(provider, isQueryCompositionMode: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="provider">The service provider to use.</param>
        /// <param name="isQueryCompositionMode">If the model is being built for only querying.</param>
        /// <remarks>The model built if <paramref name="isQueryCompositionMode"/> is <c>true</c> has more relaxed
        /// inference rules and also treats all types as entity types. This constructor is intended for use by unit testing only.</remarks>
        /// <remarks>
        /// While this function does not use types that are AspNetCore-specific,
        /// the functionality is due to the way assembly resolution is done in AspNet vs AspnetCore.
        /// </remarks>
        public ODataConventionModelBuilder(IServiceProvider provider, bool isQueryCompositionMode)
        {
            if (provider == null)
            {
                throw Error.ArgumentNull("provider");
            }

            // Create an IWebApiAssembliesResolver from configuration and initialize.
            ApplicationPartManager applicationPartManager = provider.GetRequiredService<ApplicationPartManager>();
            IWebApiAssembliesResolver internalResolver = new WebApiAssembliesResolver(applicationPartManager);
            Initialize(internalResolver, isQueryCompositionMode);
        }
    }
}
