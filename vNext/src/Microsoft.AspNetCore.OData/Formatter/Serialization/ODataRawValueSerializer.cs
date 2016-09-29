﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing the raw value of an <see cref="IEdmPrimitiveType"/>.
    /// </summary>
    public class ODataRawValueSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataRawValueSerializer"/>.
        /// </summary>
        public ODataRawValueSerializer()
            : base(ODataPayloadKind.Value)
        {
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }
            if (graph == null)
            {
                throw Error.ArgumentNull("graph");
            }

            if (graph.GetType().GetTypeInfo().IsEnum)
            {
                messageWriter.WriteValue(graph.ToString());
            }
            else
            {
                messageWriter.WriteValue(ODataPrimitiveSerializer.ConvertUnsupportedPrimitives(graph));
            }
        }
    }
}
