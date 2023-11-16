//-----------------------------------------------------------------------------
// <copyright file="ResultHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Net;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.AspNet.OData.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;
using Microsoft.OData.Edm;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData.Results
{
    internal static partial class ResultHelpers
    {
        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public static Uri GenerateODataLink(HttpRequest request, object entity, bool isEntityId)
        {
            IEdmModel model = request.GetModel();
            if (model == null)
            {
                throw new InvalidOperationException(SRResources.RequestMustHaveModel);
            }

            ODataPath path = request.ODataFeature().Path;
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
                MetadataLevel = ODataMetadataLevel.FullMetadata, // Used internally to always calculate the links.
                Request = request,
                Path = path
            };

            IEdmEntityTypeReference entityType = GetEntityType(model, entity);
            ResourceContext resourceContext = new ResourceContext(serializerContext, entityType, entity);

            return GenerateODataLink(resourceContext, isEntityId);
        }

        /// <remarks>This signature uses types that are AspNetCore-specific.</remarks>
        public static void AddEntityId(HttpResponse response, Func<Uri> entityId)
        {
            if (response.StatusCode == (int)HttpStatusCode.NoContent)
            {
                response.Headers.Add(EntityIdHeaderName, entityId().AbsoluteUri);
            }
        }

        public static void AddServiceVersion(HttpResponse response, Func<string> version)
        {
            if (response.StatusCode == (int)HttpStatusCode.NoContent)
            {
                response.Headers[ODataVersionConstraint.ODataServiceVersionHeader] = version();
            }
        }

        internal static ODataVersion GetODataVersion(HttpRequest request)
        {
            Contract.Assert(request != null, "GetODataResponseVersion called with a null request");
            return request.ODataMaxServiceVersion() ??
                request.ODataMinServiceVersion() ??
                request.ODataServiceVersion() ??
                ODataVersionConstraint.DefaultODataVersion;
        }

        internal static ODataVersion GetDeltaVersion(HttpRequest request)
        {
            ODataVersion? version = request?.GetRequestContainer()?.GetService<IODataDeltaVersionProvider>()?.Version;
            if (version != null)
            {
                return version.Value;
            }

            return ODataVersion.V401;
        }
    }
}
