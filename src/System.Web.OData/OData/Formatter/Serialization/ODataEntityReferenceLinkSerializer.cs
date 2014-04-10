// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Web.OData.Routing;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
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

                messageWriter.WriteEntityReferenceLink(entityReferenceLink);
            }
        }
    }
}
