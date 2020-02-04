// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Formatter;
using Microsoft.AspNet.OData.Formatter.Serialization;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;
using ODataPath = Microsoft.AspNet.OData.Routing.ODataPath;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Partial implementation of the ETagMessageHandler.
    /// </summary>
    public partial class ETagMessageHandler
    {
        private static EntityTagHeaderValue GetETag(
            int? statusCode,
            ODataPath path,
            IEdmModel model,
            object value,
            IETagHandler etagHandler)
        {
            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            if (etagHandler == null)
            {
                throw Error.ArgumentNull("etagHandler");
            }

            // Do not interfere with null responses, we want to bubble it up to the top.
            // Do not handle 204 responses as the spec says a 204 response must not include an ETag header
            // unless the request's representation data was saved without any transformation applied to the body
            // (i.e., the resource's new representation data is identical to the representation data received in the
            // PUT request) and the ETag value reflects the new representation.
            // Even in that case returning an ETag is optional and it requires access to the original object which is
            // not possible with the current architecture, so if the user is interested he can set the ETag in that
            // case by himself on the response.
            if (statusCode == null || !((int)statusCode.Value >= 200 && (int)statusCode.Value < 300) || statusCode.Value == (int)HttpStatusCode.NoContent)
            {
                return null;
            }

            IEdmEntityType edmType = GetSingleEntityEntityType(path);

            IEdmEntityTypeReference typeReference = GetTypeReference(model, edmType, value);
            if (typeReference != null)
            {
                ResourceContext context = CreateInstanceContext(model, typeReference, value);
                context.EdmModel = model;
                context.NavigationSource = path.NavigationSource;
                return CreateETag(context, etagHandler);
            }

            return null;
        }

        private static IEdmEntityTypeReference GetTypeReference(IEdmModel model, IEdmEntityType edmType, object value)
        {
            if (model == null || edmType == null || value == null)
            {
                return null;
            }

            IEdmObject edmObject = value as IEdmEntityObject;
            if (edmObject != null)
            {
                IEdmTypeReference edmTypeReference = edmObject.GetEdmType();
                return edmTypeReference.AsEntity();
            }

            IEdmTypeReference reference = model.GetTypeMappingCache().GetEdmType(value.GetType(), model);
            if (reference != null && reference.Definition.IsOrInheritsFrom(edmType))
            {
                return (IEdmEntityTypeReference)reference;
            }

            return null;
        }

        private static EntityTagHeaderValue CreateETag(
            ResourceContext resourceContext,
            IETagHandler handler)
        {
            IEdmModel model = resourceContext.EdmModel;
 
            IEnumerable<IEdmStructuralProperty> concurrencyProperties;
            if (model != null && resourceContext.NavigationSource != null)
            {
                concurrencyProperties = model.GetConcurrencyProperties(resourceContext.NavigationSource).OrderBy(c => c.Name);
            }
            else
            {
                concurrencyProperties = Enumerable.Empty<IEdmStructuralProperty>();
            }

            IDictionary<string, object> properties = new Dictionary<string, object>();
            foreach (IEdmStructuralProperty etagProperty in concurrencyProperties)
            {
                properties.Add(etagProperty.Name, resourceContext.GetPropertyValue(etagProperty.Name));
            }
            return handler.CreateETag(properties);
        }

        private static ResourceContext CreateInstanceContext(IEdmModel model, IEdmEntityTypeReference reference, object value)
        {
            Contract.Assert(reference != null);
            Contract.Assert(value != null);

            ODataSerializerContext serializerCtx = new ODataSerializerContext
            {
                Model = model
            };
            return new ResourceContext(serializerCtx, reference, value);
        }

        // Retrieves the IEdmEntityType from the path only in the case that we are addressing a single entity.
        // We iterate the path backwards and we return as soon as we realize we are referencing a single entity.
        // That is, as soon as we find a singleton segment, a key segment or a navigation segment with target
        // multiplicity 0..1 or 1.
        internal static IEdmEntityType GetSingleEntityEntityType(ODataPath path)
        {
            if (path == null || path.Segments.Count == 0)
            {
                return null;
            }

            int currentSegmentIndex = path.Segments.Count - 1;

            // Skip a possible sequence of casts at the end of the path.
            while (currentSegmentIndex >= 0 &&
                path.Segments[currentSegmentIndex] is TypeSegment)
            {
                currentSegmentIndex--;
            }
            if (currentSegmentIndex < 0)
            {
                return null;
            }

            ODataPathSegment currentSegment = path.Segments[currentSegmentIndex];

            if (currentSegment is SingletonSegment || currentSegment is KeySegment)
            {
                return (IEdmEntityType)path.EdmType;
            }

            NavigationPropertySegment navigationPropertySegment = currentSegment as NavigationPropertySegment;
            if (navigationPropertySegment != null)
            {
                if (navigationPropertySegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.ZeroOrOne ||
                    navigationPropertySegment.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.One)
                {
                    return (IEdmEntityType)path.EdmType;
                }
            }

            return null;
        }
    }
}
