//-----------------------------------------------------------------------------
// <copyright file="ODataEntityReferenceLinkSerializer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing $ref response.
    /// </summary>
    // For example, the response to the url http://localhost/Products(10)/Category/$ref gets serialized using this.</remarks>
    public class ODataEntityReferenceLinkSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ODataEntityReferenceLinkSerializer"/>.
        /// </summary>
        public ODataEntityReferenceLinkSerializer()
            : base(ODataPayloadKind.EntityReferenceLink)
        {
        }

        /// <inheritdoc/>
        public override void WriteObject(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            if (graph != null)
            {
                ODataEntityReferenceLink entityReferenceLink = GetEntityReferenceLink(graph);
                messageWriter.WriteEntityReferenceLink(entityReferenceLink);
            }
        }

        /// <inheritdoc/>
        public override Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            if (graph != null)
            {
                ODataEntityReferenceLink entityReferenceLink = GetEntityReferenceLink(graph);
                return messageWriter.WriteEntityReferenceLinkAsync(entityReferenceLink);
            }

            return TaskHelpers.Completed();
        }

        private ODataEntityReferenceLink GetEntityReferenceLink(object graph)
        {
            ODataEntityReferenceLink entityReferenceLink = graph as ODataEntityReferenceLink;
            if (entityReferenceLink == null)
            {
                Uri uri = graph as Uri;
                if (uri == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
                }

                entityReferenceLink = new ODataEntityReferenceLink { Url = uri };
            }

            return entityReferenceLink;
        }
    }
}
