// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
    }
}
