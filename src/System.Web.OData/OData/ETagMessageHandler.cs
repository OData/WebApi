// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Serialization;
using System.Web.OData.Properties;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

namespace System.Web.OData
{
    /// <summary>
    /// Defines a <see cref="HttpMessageHandler"/> to add an ETag header value to an OData response when the response 
    /// is a single entity that has an ETag defined.
    /// </summary>
    public class ETagMessageHandler : DelegatingHandler
    {
        /// <inheritdoc/>
        protected async override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw Error.ArgumentNull("request");
            }

            HttpConfiguration configuration = request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            // Do not interfere with null responses, we want to buble it up to the top.
            // Do not handle 204 responses as the spec says a 204 response must not include an ETag header
            // unless the request's representation data was saved without any transformation applied to the body
            // (i.e., the resource's new representation data is identical to the representation data received in the
            // PUT request) and the ETag value reflects the new representation.
            // Even in that case returning an ETag is optional and it requires access to the original object which is 
            // not possible with the current architecture, so if the user is interested he can set the ETag in that
            // case by himself on the response.
            if (response == null || !response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.NoContent)
            {
                return response;
            }

            ODataPath path = request.ODataProperties().Path;
            IEdmModel model = request.ODataProperties().Model;

            IEdmEntityType edmType = GetSingleEntityEntityType(path);
            object value = GetSingleEntityObject(response);

            IEdmEntityTypeReference typeReference = GetTypeReference(model, edmType, value);
            if (typeReference != null)
            {
                EntityInstanceContext context = CreateInstanceContext(typeReference, value);
                IETagHandler etagHandler = configuration.GetETagHandler();
                EntityTagHeaderValue etag = CreateETag(context, etagHandler);

                if (etag != null)
                {
                    response.Headers.ETag = etag;
                }
            }

            return response;
        }

        private static IEdmEntityTypeReference GetTypeReference(IEdmModel model, IEdmEntityType edmType, object value)
        {
            if (model == null || edmType == null || value == null)
            {
                return null;
            }

            IEdmTypeReference reference = EdmLibHelpers.GetEdmTypeReference(model, value.GetType());
            if (reference != null && reference.Definition.IsOrInheritsFrom(edmType))
            {
                return (IEdmEntityTypeReference)reference;
            }

            return null;
        }

        // This overload is for unit testing purposes only.
        internal Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return SendAsync(request, CancellationToken.None);
        }

        private static EntityTagHeaderValue CreateETag(
            EntityInstanceContext entityInstanceContext,
            IETagHandler handler)
        {
            IEnumerable<IEdmStructuralProperty> concurrencyProperties =
                entityInstanceContext.EntityType.GetConcurrencyProperties().OrderBy(c => c.Name);

            IDictionary<string, object> properties = new Dictionary<string, object>();
            foreach (IEdmStructuralProperty etagProperty in concurrencyProperties)
            {
                properties.Add(etagProperty.Name, entityInstanceContext.GetPropertyValue(etagProperty.Name));
            }
            EntityTagHeaderValue etagHeaderValue = handler.CreateETag(properties);
            return etagHeaderValue;
        }

        private static object GetSingleEntityObject(HttpResponseMessage response)
        {
            Contract.Assert(response != null);

            ObjectContent content = response.Content as ObjectContent;
            if (content != null)
            {
                return content.Value;
            }

            return null;
        }

        private static EntityInstanceContext CreateInstanceContext(IEdmEntityTypeReference reference, object value)
        {
            Contract.Assert(reference != null);
            Contract.Assert(value != null);

            ODataSerializerContext serializerCtx = new ODataSerializerContext();
            return new EntityInstanceContext(serializerCtx, reference, value);
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
                path.Segments[currentSegmentIndex].SegmentKind == ODataSegmentKinds._Cast)
            {
                currentSegmentIndex--;
            }
            if (currentSegmentIndex < 0)
            {
                return null;
            }

            ODataPathSegment currentSegment = path.Segments[currentSegmentIndex];
            switch (currentSegment.SegmentKind)
            {
                case ODataSegmentKinds._Singleton:
                case ODataSegmentKinds._Key:
                    return (IEdmEntityType)path.EdmType;

                case ODataSegmentKinds._Navigation:
                    NavigationPathSegment navigation = (NavigationPathSegment)currentSegment;
                    if (navigation.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.ZeroOrOne ||
                        navigation.NavigationProperty.TargetMultiplicity() == EdmMultiplicity.One)
                    {
                        return (IEdmEntityType)path.EdmType;
                    }
                    break;
                default:
                    break;
            }
            return null;
        }
    }
}
