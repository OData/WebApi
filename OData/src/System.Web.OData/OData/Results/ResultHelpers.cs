// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Routing;
using System.Web.OData.Builder;
using System.Web.OData.Builder.Conventions;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = System.Web.OData.Routing.ODataPath;

namespace System.Web.OData.Results
{
    internal static class ResultHelpers
    {
        public const string EntityIdHeaderName = "OData-EntityId";

        public static Uri GenerateODataLink(HttpRequestMessage request, object entity, bool isEntityId)
        {
            EntityContext entityContext = CreateEntityContext(request, entity);
            Contract.Assert(entityContext != null);

            // Generate location or entityId header from request Uri and key, if Post to a containment.
            // Link builder is not used, since it is also for generating ID, Edit, Read links, etc. scenarios, where
            // request Uri is not used.
            if (entityContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.ContainedEntitySet)
            {
                return GenerateContainmentODataPathSegments(entityContext, isEntityId);
            }

            NavigationSourceLinkBuilderAnnotation linkBuilder =
                entityContext.EdmModel.GetNavigationSourceLinkBuilder(entityContext.NavigationSource);
            Contract.Assert(linkBuilder != null);

            Uri idLink = linkBuilder.BuildIdLink(entityContext);
            if (isEntityId)
            {
                if (idLink == null)
                {
                    throw Error.InvalidOperation(
                        SRResources.IdLinkNullForEntityIdHeader,
                        entityContext.NavigationSource.Name);
                }

                return idLink;
            }

            Uri editLink = linkBuilder.BuildEditLink(entityContext);
            if (editLink == null)
            {
                if (idLink != null)
                {
                    return idLink;
                }

                throw Error.InvalidOperation(
                    SRResources.EditLinkNullForLocationHeader,
                    entityContext.NavigationSource.Name);
            }

            return editLink;
        }

        public static void AddEntityId(HttpResponseMessage response, Func<Uri> entityId)
        {
            if (response.StatusCode == HttpStatusCode.NoContent)
            {
                response.Headers.TryAddWithoutValidation(EntityIdHeaderName, entityId().ToString());
            }
        }

        private static Uri GenerateContainmentODataPathSegments(EntityContext entityContext, bool isEntityId)
        {
            Contract.Assert(entityContext != null);
            Contract.Assert(
                entityContext.NavigationSource.NavigationSourceKind() == EdmNavigationSourceKind.ContainedEntitySet);
            Contract.Assert(entityContext.Request != null);

            ODataPath path = entityContext.Request.ODataProperties().Path;
            if (path == null)
            {
                throw Error.InvalidOperation(SRResources.ODataPathMissing);
            }

            path = new ContainmentPathBuilder().TryComputeCanonicalContainingPath(path);

            List<ODataPathSegment> odataPath = path.Segments.ToList();

            // create a template entity set if it's contained entity set
            IEdmEntitySet entitySet = entityContext.NavigationSource as IEdmEntitySet;
            if (entitySet == null)
            {
                EdmEntityContainer container = new EdmEntityContainer("NS", "Default");
                entitySet = new EdmEntitySet(container, entityContext.NavigationSource.Name, entityContext.NavigationSource.EntityType());
            }

            odataPath.Add(new EntitySetSegment(entitySet));
            odataPath.Add(new KeySegment(ConventionsHelpers.GetEntityKey(entityContext), entityContext.EntityType, entityContext.NavigationSource));

            if (!isEntityId)
            {
                bool isSameType = entityContext.EntityType == entityContext.NavigationSource.EntityType();
                if (!isSameType)
                {
                    odataPath.Add(new TypeSegment(entityContext.EntityType, entityContext.NavigationSource));
                }
            }

            string odataLink = entityContext.Url.CreateODataLink(odataPath);
            return odataLink == null ? null : new Uri(odataLink);
        }

        private static EntityContext CreateEntityContext(HttpRequestMessage request, object entity)
        {
            IEdmModel model = request.ODataProperties().Model;
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
                RequestContext = request.GetRequestContext(),
                Path = path
            };

            IEdmEntityTypeReference entityType = GetEntityType(model, entity);
            return new EntityContext(serializerContext, entityType, entity);
        }

        private static IEdmEntityTypeReference GetEntityType(IEdmModel model, object entity)
        {
            Type entityType = entity.GetType();
            IEdmTypeReference edmType = model.GetEdmTypeReference(entityType);
            if (edmType == null)
            {
                throw Error.InvalidOperation(SRResources.EntityTypeNotInModel, entityType.FullName);
            }
            if (!edmType.IsEntity())
            {
                throw Error.InvalidOperation(SRResources.TypeMustBeEntity, edmType.FullName());
            }

            return edmType.AsEntity();
        }
    }
}
