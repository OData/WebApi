// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public partial class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequestMessage request)
        {
            // Using a Func<IEdmModel> to delay evaluation of the model.
            return GetODataPayloadSerializerImpl(type, () => request.GetModel(), request.ODataProperties().Path, typeof(HttpError));
        }
    }
}
