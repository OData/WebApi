// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// Represents an <see cref="ODataSerializer"/> for serializing $link response for a collection navigation property.
    /// </summary>
    public class ODataEntityReferenceLinksSerializer : ODataSerializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntityReferenceLinksSerializer"/> class.
        /// </summary>
        public ODataEntityReferenceLinksSerializer()
            : base(ODataPayloadKind.EntityReferenceLinks)
        {
        }

        /// <inheridoc />
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

            IEdmEntitySet entitySet = writeContext.EntitySet;
            if (entitySet == null)
            {
                throw new SerializationException(SRResources.EntitySetMissingDuringSerialization);
            }

            if (writeContext.Path == null)
            {
                throw new SerializationException(SRResources.ODataPathMissing);
            }

            IEdmNavigationProperty navigationProperty = writeContext.Path.GetNavigationProperty();
            if (navigationProperty == null)
            {
                throw new SerializationException(SRResources.NavigationPropertyMissingDuringSerialization);
            }

            if (graph != null)
            {
                ODataEntityReferenceLinks entityReferenceLinks = graph as ODataEntityReferenceLinks;
                if (entityReferenceLinks == null)
                {
                    IEnumerable<Uri> uris = graph as IEnumerable<Uri>;
                    if (uris == null)
                    {
                        throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
                    }

                    entityReferenceLinks = new ODataEntityReferenceLinks
                    {
                        Links = uris.Select(uri => new ODataEntityReferenceLink { Url = uri })
                    };
                }

                messageWriter.WriteEntityReferenceLinks(entityReferenceLinks, entitySet, navigationProperty);
            }
        }
    }
}
