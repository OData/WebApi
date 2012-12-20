// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataEntityReferenceLinkDeserializer : ODataDeserializer
    {
        public ODataEntityReferenceLinkDeserializer()
            : base(ODataPayloadKind.EntityReferenceLink)
        { 
        }

        public override object Read(ODataMessageReader messageReader, ODataDeserializerContext readContext)
        {
            if (messageReader == null)
            {
                throw Error.ArgumentNull("messageReader");
            }

            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            IEdmNavigationProperty navigationProperty = GetNavigationProperty(readContext.Path);

            if (navigationProperty == null)
            {
                throw new SerializationException(SRResources.NavigationPropertyMissingDuringDeserialization);
            }

            ODataEntityReferenceLink entityReferenceLink = messageReader.ReadEntityReferenceLink(navigationProperty);

            if (entityReferenceLink != null)
            {
                return entityReferenceLink.Url;
            }

            return null;
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
