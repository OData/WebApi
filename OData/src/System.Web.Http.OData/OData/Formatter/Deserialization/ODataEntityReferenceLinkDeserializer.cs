// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http.OData.Batch;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Routing;
using System.Web.Http.Routing;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData entity reference link payloads.
    /// </summary>
    public class ODataEntityReferenceLinkDeserializer : ODataDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntityReferenceLinkDeserializer"/> class.
        /// </summary>
        public ODataEntityReferenceLinkDeserializer()
            : base(ODataPayloadKind.EntityReferenceLink)
        {
        }

        /// <inheritdoc />
        public override object Read(ODataMessageReader messageReader, Type type, ODataDeserializerContext readContext)
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
                return ResolveContentId(entityReferenceLink.Url, readContext);
            }

            return null;
        }

        private static Uri ResolveContentId(Uri uri, ODataDeserializerContext readContext)
        {
            if (uri != null)
            {
                IDictionary<string, string> contentIDToLocationMapping = readContext.Request.GetODataContentIdMapping();
                if (contentIDToLocationMapping != null)
                {
                    UrlHelper urlHelper = readContext.Request.GetUrlHelper() ?? new UrlHelper(readContext.Request);
                    Uri baseAddress = new Uri(urlHelper.CreateODataLink());
                    string relativeUrl = uri.IsAbsoluteUri ? baseAddress.MakeRelativeUri(uri).OriginalString : uri.OriginalString;
                    string resolvedUrl = ContentIdHelpers.ResolveContentId(relativeUrl, contentIDToLocationMapping);
                    Uri resolvedUri = new Uri(resolvedUrl, UriKind.RelativeOrAbsolute);
                    if (!resolvedUri.IsAbsoluteUri)
                    {
                        resolvedUri = new Uri(baseAddress, uri);
                    }
                    return resolvedUri;
                }
            }

            return uri;
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