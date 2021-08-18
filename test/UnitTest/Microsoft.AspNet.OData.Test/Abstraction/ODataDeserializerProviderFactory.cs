//-----------------------------------------------------------------------------
// <copyright file="ODataDeserializerProviderFactory.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData.Formatter.Deserialization;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNet.OData.Test.Abstraction
{
    /// <summary>
    /// A factory for creating <see cref="ODataDeserializerProvider"/>.
    /// </summary>
    public class ODataDeserializerProviderFactory
    {
        /// <summary>
        /// Create an <see cref="ODataDeserializerProvider"/>.
        /// </summary>
        /// <returns>An ODataDeserializerProvider.</returns>
        public static ODataDeserializerProvider Create()
        {
            return new MockContainer().GetRequiredService<ODataDeserializerProvider>();
        }
    }
}
