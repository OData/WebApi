﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.OData.Edm;

namespace System.Web.OData
{
    /// <summary>
    /// Represents an instance of an <see cref="IEdmStructuredType"/>.
    /// </summary>
    public interface IEdmStructuredObject : IEdmObject
    {
        /// <summary>
        /// Gets the value of the property with the given name.
        /// </summary>
        /// <param name="propertyName">The name of the property to get.</param>
        /// <param name="value">When this method returns, contains the value of the property with the given name, if the property is found;
        /// otherwise, null. The parameter is passed uninitialized.</param>
        /// <returns><c>true</c> if the instance contains the property with the given name; otherwise, <c>false</c>.</returns>
        [SuppressMessage("Microsoft.Design", "CA1007:UseGenericsWhereAppropriate",
            Justification = "Generics not appropriate here as this interface supports typeless")]
        bool TryGetPropertyValue(string propertyName, out object value);
    }
}
