// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing $links response.
    /// </summary>
    /// <remarks>For example, the response to the url http://localhost/Products(10)/$links/Category gets serialized using this.</remarks>
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
        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
        {
            if (messageWriter == null)
            {
                throw Error.ArgumentNull("messageWriter");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            IEdmEntitySet entitySet = writeContext.EntitySet;

            if (entitySet == null)
            {
                throw new SerializationException(SRResources.EntitySetMissingDuringSerialization);
            }

            IEdmNavigationProperty navigationProperty = GetNavigationProperty(writeContext.Path);

            if (navigationProperty == null)
            {
                throw new SerializationException(SRResources.NavigationPropertyMissingDuringSerialization);
            }

            if (graph != null)
            {
                Uri uri = graph as Uri;
                if (uri == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
                }

                messageWriter.WriteEntityReferenceLink(new ODataEntityReferenceLink { Url = uri }, entitySet, navigationProperty);
            }
        }

        private static IEdmNavigationProperty GetNavigationProperty(ODataPath path)
        {
            if (path == null)
            {
                throw new SerializationException(SRResources.ODataPathMissing);
            }

            return path.GetNavigationProperty();
        }
    }
}
