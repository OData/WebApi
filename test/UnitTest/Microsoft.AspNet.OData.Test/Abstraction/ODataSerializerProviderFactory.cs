//-----------------------------------------------------------------------------
// <copyright file="ODataSerializerProviderFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A factory for creating <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public static class ODataSerializerProviderFactory
    {
        /// <summary>
        /// Create an <see cref="ODataSerializerProvider"/>.
        /// </summary>
        /// <returns>An ODataSerializerProvider.</returns>
        public static ODataSerializerProvider Create()
        {
            return new MockContainer().GetRequiredService<ODataSerializerProvider>();
        }
    }
}
