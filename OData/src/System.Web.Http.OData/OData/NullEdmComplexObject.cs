// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;

namespace System.Web.Http.OData
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
