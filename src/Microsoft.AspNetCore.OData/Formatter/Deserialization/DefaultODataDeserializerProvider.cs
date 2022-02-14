//-----------------------------------------------------------------------------
// <copyright file="DefaultODataDeserializerProvider.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNet.OData.Formatter.Deserialization
{
    /// <summary>
    /// The default <see cref="ODataDeserializerProvider"/>.
    /// </summary>
    public partial class DefaultODataDeserializerProvider : ODataDeserializerProvider
    {
        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public override ODataDeserializer GetODataDeserializer(Type type, HttpRequest request)
        {
            // Using a Func<IEdmModel> to delay evaluation of the model.
            return GetODataDeserializerImpl(type, () => request.GetModel());
        }
    }
}
