// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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
