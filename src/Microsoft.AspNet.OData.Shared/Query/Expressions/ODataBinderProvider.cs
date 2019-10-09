// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// An ODataBinderProvider is a factory for creating OData binders.
    /// </summary>
    public abstract class ODataBinderProvider
    {
        /// <summary>
        /// Gets a <see cref="SelectExpandBinder"/>.
        /// </summary>
        /// <param name="settings">The <see cref="ODataQuerySettings"/> to use during binding.</param>
        /// <param name="selectExpandQuery">The <see cref="SelectExpandQueryOption"/> that contains the OData $select and $expand query options.</param>
        /// <returns>The <see cref="SelectExpandBinder"/>.</returns>
        public abstract SelectExpandBinder GetSelectExpandBinder(ODataQuerySettings settings,
            SelectExpandQueryOption selectExpandQuery);

        /// <summary>
        /// Gets a <see cref="AggregationBinder"/>.
        /// </summary>
        /// <param name="settings">The <see cref="ODataQuerySettings"/> to use during binding.</param>
        /// <param name="elementType">ClrType for result of transformations.</param>
        /// <param name="requestContainer">The request container.</param>
        /// <param name="model">The EDM model.</param>
        /// <param name="transformation">The transformation node.</param>
        /// <returns>The <see cref="AggregationBinder"/>.</returns>
        public abstract AggregationBinder GetAggregationBinder(ODataQuerySettings settings, IServiceProvider requestContainer, 
            Type elementType, IEdmModel model, TransformationNode transformation);
    }
}
