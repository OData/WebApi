//-----------------------------------------------------------------------------
// <copyright file="IExpandQueryBuilder.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Exposes the ability to generate a $expand query string from a payload object.
    /// </summary>
    public interface IExpandQueryBuilder
    {
        /// <summary>
        /// Generates a $expand query string from a payload object.
        /// </summary>
        /// <param name="value">The payload object.</param>
        /// <param name="model">The service model.</param>
        /// <returns>A $expand query string.</returns>
        string GenerateExpandQueryParameter(object value, IEdmModel model);
    }
}
