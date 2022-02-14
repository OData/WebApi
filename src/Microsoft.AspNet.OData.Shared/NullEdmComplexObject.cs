//-----------------------------------------------------------------------------
// <copyright file="NullEdmComplexObject.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents an <see cref="IEdmComplexObject"/> that is null.
    /// </summary>
    public class NullEdmComplexObject : IEdmComplexObject
    {
        private IEdmComplexTypeReference _edmType;

        /// <summary>
        /// Initializes a new instance of the <see cref="NullEdmComplexObject"/> class.
        /// </summary>
        /// <param name="edmType">The EDM type of this object.</param>
        public NullEdmComplexObject(IEdmComplexTypeReference edmType)
        {
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }

            _edmType = edmType;
        }

        /// <inheritdoc/>
        public bool TryGetPropertyValue(string propertyName, out object value)
        {
            throw Error.InvalidOperation(SRResources.EdmComplexObjectNullRef, propertyName, _edmType.ToTraceString());
        }

        /// <inheritdoc/>
        public IEdmTypeReference GetEdmType()
        {
            return _edmType;
        }
    }
}
