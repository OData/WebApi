//-----------------------------------------------------------------------------
// <copyright file="ODataConventionModelBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Web.Http;
using System.Web.Http.Dispatcher;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;

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
        /// <see cref="DefaultAssembliesResolver"/> to get a list of assemblies to build
        /// the model.
        /// </remarks>
        public ODataConventionModelBuilder()
            : this(WebApiAssembliesResolver.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <remarks>This function uses types that are AspNet-specific.</remarks>
        public ODataConventionModelBuilder(HttpConfiguration configuration)
            : this(configuration, isQueryCompositionMode: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="isQueryCompositionMode">If the model is being built for only querying.</param>
        /// <remarks>The model built if <paramref name="isQueryCompositionMode"/> is <c>true</c> has more relaxed
        /// inference rules and also treats all types as entity types. This constructor is intended for use by unit testing only.</remarks>
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public ODataConventionModelBuilder(HttpConfiguration configuration, bool isQueryCompositionMode)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            // Create an IWebApiAssembliesResolver from configuration and initialize.
            IAssembliesResolver aspnetResolver = configuration.Services.GetAssembliesResolver();
            IWebApiAssembliesResolver internalResolver = new WebApiAssembliesResolver(aspnetResolver);
            Initialize(internalResolver, isQueryCompositionMode);
        }
    }
}
