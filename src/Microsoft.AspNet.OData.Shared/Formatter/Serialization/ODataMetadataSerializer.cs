﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing $metadata. 
    /// </summary>
    public class ODataMetadataSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataMetadataSerializer"/>.
        /// </summary>
        public ODataMetadataSerializer()
            : base(ODataPayloadKind.MetadataDocument)
        {
        }

        /// <inheritdoc/>
        /// <remarks>The metadata written is from the model set on the <paramref name="messageWriter"/>. The <paramref name="graph" />
        /// is not used.</remarks>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            // NOTE: ODataMessageWriter doesn't have a way to set the IEdmModel. So, there is an underlying assumption here that
            // the model received by this method and the model passed(from configuration) while building ODataMessageWriter is the same (clr object).
            messageWriter.WriteMetadataDocument();
        }
    }
}
