﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// The default <see cref="ODataSerializerProvider"/>.
    /// </summary>
    public partial class DefaultODataSerializerProvider : ODataSerializerProvider
    {
        /// <inheritdoc />
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public override ODataSerializer GetODataPayloadSerializer(Type type, HttpRequest request)
        {
            // Using a Func<IEdmModel> to delay evaluation of the model.
            return GetODataPayloadSerializerImpl(type, () => request.GetModel(), request.ODataFeature().Path, typeof(SerializableError));
        }
    }
}
