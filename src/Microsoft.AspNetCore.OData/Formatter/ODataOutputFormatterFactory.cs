// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Formatter
{
    /// <summary>
    /// Factory for <see cref="ODataOutputFormatter"/> classes to handle OData.
    /// </summary>
    public static class ODataOutputFormatterFactory
    {
        /// <summary>
        /// Creates a list of media type formatters to handle OData.
        /// The default serializer provider is <see cref="ODataSerializerProviderProxy"/> and the default deserializer provider is
        /// <see cref="ODataDeserializerProviderProxy"/>.
        /// </summary>
        /// <returns>A list of media type formatters to handle OData.</returns>
        public static IList<ODataOutputFormatter> Create()
        {
            throw new NotImplementedException();
        }
    }
}
