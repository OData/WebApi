// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Adapters;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Extensions;

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
        /// <param name="configuration">The <see cref="IAssembliesResolver"/> to use.</param>
        public ODataConventionModelBuilder(IServiceProvider provider)
            : this(provider, isQueryCompositionMode: false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataConventionModelBuilder"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="HttpConfiguration"/> to use.</param>
        /// <param name="isQueryCompositionMode">If the model is being built for only querying.</param>
        /// <remarks>The model built if <paramref name="isQueryCompositionMode"/> is <c>true</c> has more relaxed
        /// inference rules and also treats all types as entity types. This constructor is intended for use by unit testing only.</remarks>
        public ODataConventionModelBuilder(IServiceProvider provider, bool isQueryCompositionMode)
        {
            if (provider == null)
            {
                throw Error.ArgumentNull("provider");
            }

            Initialize(provider.GetWebApiAssembliesResolver(), isQueryCompositionMode);
        }
    }
}
