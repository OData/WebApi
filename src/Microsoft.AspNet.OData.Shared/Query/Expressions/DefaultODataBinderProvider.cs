// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser.Aggregation;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// The default <see cref="ODataBinderProvider"/>.
    /// </summary>
    public class DefaultODataBinderProvider : ODataBinderProvider
    {
        /// <inheritdoc />
        public override SelectExpandBinder GetSelectExpandBinder(ODataQuerySettings settings, SelectExpandQueryOption selectExpandQuery)
        {
            return new SelectExpandBinder(settings, selectExpandQuery);
        }

        /// <inheritdoc />
        public override AggregationBinder GetAggregationBinder(ODataQuerySettings settings, IServiceProvider requestContainer, Type elementType,
            IEdmModel model, TransformationNode transformation)
        {
            return new AggregationBinder(settings, requestContainer, elementType, model, transformation);
        }
    }
}
