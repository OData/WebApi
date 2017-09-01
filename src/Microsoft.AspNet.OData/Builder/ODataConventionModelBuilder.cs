// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
        public ODataConventionModelBuilder()
            : this(new WebApiAssembliesResolver(new DefaultAssembliesResolver()))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="IAssembliesResolver"/> to use.</param>
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
        public ODataConventionModelBuilder(HttpConfiguration configuration, bool isQueryCompositionMode)
        {
            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            IWebApiAssembliesResolver resolver =
                new WebApiAssembliesResolver(configuration.Services.GetAssembliesResolver());
            Initialize(resolver, isQueryCompositionMode);
        }
    }
}
