// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Routing;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Results
{
    internal static partial class ResultHelpers
    {
        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public static Uri GenerateODataLink(HttpRequestMessage request, object entity, bool isEntityId)
        {
            IEdmModel model = request.GetModel();
            if (model == null)
            {
                throw new InvalidOperationException(SRResources.RequestMustHaveModel);
            }

            ODataPath path = request.ODataProperties().Path;
            if (path == null)
            {
                throw new InvalidOperationException(SRResources.ODataPathMissing);
            }

            IEdmNavigationSource navigationSource = path.NavigationSource;
            if (navigationSource == null)
            {
                throw new InvalidOperationException(SRResources.NavigationSourceMissingDuringSerialization);
            }

            ODataSerializerContext serializerContext = new ODataSerializerContext
            {
                NavigationSource = navigationSource,
                Model = model,
                Url = request.GetUrlHelper() ?? new UrlHelper(request),
                MetadataLevel = ODataMetadataLevel.FullMetadata, // Used internally to always calculate the links.
                Request = request,
                Path = path
            };

            IEdmEntityTypeReference entityType = GetEntityType(model, entity);
            ResourceContext resourceContext = new ResourceContext(serializerContext, entityType, entity);

            return GenerateODataLink(resourceContext, isEntityId);
        }

        /// <remarks>This signature uses types that are AspNet-specific.</remarks>
        public static void AddEntityId(HttpResponseMessage response, Func<Uri> entityId)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                Uri location = entityId();
                response.Headers.TryAddWithoutValidation(EntityIdHeaderName, location.AbsoluteUri);
            }
        }

        public static void AddServiceVersion(HttpResponseMessage response, Func<string> version)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                response.Headers.TryAddWithoutValidation(ODataVersionConstraint.ODataServiceVersionHeader, version());
            }
        }

        internal static ODataVersion GetODataResponseVersion(HttpRequestMessage request)
        {
            if (request == null)
            {
                return ODataVersionConstraint.DefaultODataVersion;
            }

            HttpRequestMessageProperties properties = request.ODataProperties();
            return properties.ODataMaxServiceVersion ??
                properties.ODataMinServiceVersion ??
                properties.ODataServiceVersion ??
                ODataVersionConstraint.DefaultODataVersion;
        }
    }
}
