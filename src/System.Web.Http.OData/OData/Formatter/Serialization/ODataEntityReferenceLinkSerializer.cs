// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Serialization
{
    /// <summary>
    /// ODataSerializer for serializing navigation links. For example, the response to the url
    /// http://localhost/Products(10)/$links/Category gets serialized using this.
    /// </summary>
    internal class ODataEntityReferenceLinkSerializer : ODataSerializer
    {
        public ODataEntityReferenceLinkSerializer()
            : base(ODataPayloadKind.EntityReferenceLink)
        {
        }

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

            IEdmNavigationProperty navigationProperty = GetNavigationPropertyOrDefault(writeContext.Path);

            if (navigationProperty == null)
            {
                throw new SerializationException(SRResources.NavigationPropertyMissingDuringSerialization);
            }

            messageWriter.WriteEntityReferenceLink(new ODataEntityReferenceLink { Url = graph as Uri }, entitySet,
                navigationProperty);
        }

        private static IEdmNavigationProperty GetNavigationPropertyOrDefault(ODataPath path)
        {
            if (path == null)
            {
                throw new SerializationException(SRResources.ODataPathMissing);
            }

            Contract.Assert(path.Segments != null);
            NavigationPathSegment navigationSegment = path.Segments.LastOrDefault() as NavigationPathSegment;

            if (navigationSegment == null)
            {
                return null;
            }

            return navigationSegment.NavigationProperty;
        }
    }
}
