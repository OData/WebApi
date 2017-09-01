// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// An ODataSerializerProvider is a factory for creating <see cref="ODataSerializer"/>s.
    /// </summary>
    public abstract partial class ODataSerializerProvider
    {
        /// <summary>
        /// Gets an <see cref="ODataEdmTypeSerializer"/> for the given edmType.
        /// </summary>
        /// <param name="edmType">The <see cref="IEdmTypeReference"/>.</param>
        /// <returns>The <see cref="ODataSerializer"/>.</returns>
        public abstract ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType);
    }
}
