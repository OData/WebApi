// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Interfaces;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" /> or <see cref="IEdmComplexType"/>
    /// </summary>
    public class ODataResourceSetSerializer : ODataEdmTypeSerializer
    {
        private const string ResourceSet = "ResourceSet";

        /// <summary>
        /// Initializes a new instance of <see cref="ODataResourceSetSerializer"/>.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public ODataResourceSetSerializer(ODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.ResourceSet, serializerProvider)
        {
        }

        /// <inheritdoc />
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

            IEdmEntitySetBase entitySet = writeContext.NavigationSource as IEdmEntitySetBase;

            IEdmTypeReference resourceSetType = writeContext.GetEdmType(graph, type);
            Contract.Assert(resourceSetType != null);

            IEdmStructuredTypeReference resourceType = GetResourceType(resourceSetType);
            ODataWriter writer = messageWriter.CreateODataResourceSetWriter(entitySet, resourceType.StructuredDefinition());
            WriteObjectInline(graph, resourceSetType, writer, writeContext);
        }

        /// <inheritdoc />
        public override void WriteObjectInline(object graph, IEdmTypeReference expectedType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }
            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }
            if (expectedType == null)
            {
                throw Error.ArgumentNull("expectedType");
            }
            if (graph == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, ResourceSet));
            }

            IEnumerable enumerable = graph as IEnumerable; // Data to serialize
            if (enumerable == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            WriteResourceSet(enumerable, expectedType, writer, writeContext);
        }

        private void WriteResourceSet(IEnumerable enumerable, IEdmTypeReference resourceSetType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(writer != null);
            Contract.Assert(writeContext != null);
            Contract.Assert(enumerable != null);
            Contract.Assert(resourceSetType != null);

            IEdmStructuredTypeReference elementType = GetResourceType(resourceSetType);
            ODataPagedResourceSet pagedResourceSet = CreatePagedResourceSet(enumerable, resourceSetType.AsCollection(), writeContext);
            ODataResourceSet resourceSet = pagedResourceSet.ResourceSet;

            if (resourceSet == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, ResourceSet));
            }

            IEdmEntitySetBase entitySet = writeContext.NavigationSource as IEdmEntitySetBase;
            if (entitySet == null)
            {
                resourceSet.SetSerializationInfo(new ODataResourceSerializationInfo
                {
                    IsFromCollection = true,
                    NavigationSourceEntityTypeName = elementType.FullName(),
                    NavigationSourceKind = EdmNavigationSourceKind.UnknownEntitySet,
                    NavigationSourceName = null
                });
            }

            ODataEdmTypeSerializer resourceSerializer = SerializerProvider.GetEdmTypeSerializer(elementType);
            if (resourceSerializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName()));
            }

            // save this for later to support JSON odata.streaming.
            Uri nextPageLink = resourceSet.NextPageLink;
            resourceSet.NextPageLink = null;
            writer.WriteStart(resourceSet);
            object lastMember = null;
            foreach (object item in enumerable)
            {
                lastMember = item;
                if (item == null || item is NullEdmComplexObject)
                {
                    if (elementType.IsEntity())
                    {
                        throw new SerializationException(SRResources.NullElementInCollection);
                    }

                    // for null complex element, it can be serialized as "null" in the collection.
                    writer.WriteStart(resource: null);
                    writer.WriteEnd();
                }
                else
                {
                    resourceSerializer.WriteObjectInline(item, elementType, writer, writeContext);
                }
            }

            // Subtle and surprising behavior: If the NextPageLink property is set before calling WriteStart(resourceSet),
            // the next page link will be written early in a manner not compatible with odata.streaming=true. Instead, if
            // the next page link is not set when calling WriteStart(resourceSet) but is instead set later on that resourceSet
            // object before calling WriteEnd(), the next page link will be written at the end, as required for
            // odata.streaming=true support.

            if (nextPageLink != null)
            {
                resourceSet.NextPageLink = nextPageLink;
            }
            else if (pagedResourceSet.NextLinkFunction != null)
            {
                resourceSet.NextPageLink = pagedResourceSet.NextLinkFunction(lastMember);
            }
            writer.WriteEnd();
        }

        /// <summary>
        /// Create the <see cref="ODataResourceSet"/> to be written for the given resourceSet instance.
        /// </summary>
        /// <param name="resourceSetInstance">The instance representing the resourceSet being written.</param>
        /// <param name="resourceSetType">The EDM type of the resourceSet being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataResourceSet"/> object.</returns>
        public virtual ODataResourceSet CreateResourceSet(IEnumerable resourceSetInstance, IEdmCollectionTypeReference resourceSetType,
            ODataSerializerContext writeContext)
        {
            ODataResourceSet resourceSet = new ODataResourceSet
            {
                TypeName = resourceSetType.FullName()
            };

            IEdmStructuredTypeReference structuredType = GetResourceType(resourceSetType).AsStructured();
            if (writeContext.NavigationSource != null && structuredType.IsEntity())
            {
                ResourceSetContext resourceSetContext = ResourceSetContext.Create(writeContext, resourceSetInstance);
                IEdmEntityType entityType = structuredType.AsEntity().EntityDefinition();
                var operations = writeContext.Model.GetAvailableOperationsBoundToCollection(entityType);
                var odataOperations = CreateODataOperations(operations, resourceSetContext, writeContext);
                foreach (var odataOperation in odataOperations)
                {
                    ODataAction action = odataOperation as ODataAction;
                    if (action != null)
                    {
                        resourceSet.AddAction(action);
                    }
                    else
                    {
                        resourceSet.AddFunction((ODataFunction)odataOperation);
                    }
                }
            }

            if (writeContext.ExpandedResource == null)
            {
                // If we have more OData format specific information apply it now, only if we are the root feed.
                PageResult odataResourceSetAnnotations = resourceSetInstance as PageResult;
                if (odataResourceSetAnnotations != null)
                {
                    resourceSet.Count = odataResourceSetAnnotations.Count;
                    resourceSet.NextPageLink = odataResourceSetAnnotations.NextPageLink;
                }
                else if (writeContext.Request != null)
                {
                    resourceSet.NextPageLink = writeContext.InternalRequest.Context.NextLink;
                    resourceSet.DeltaLink = writeContext.InternalRequest.Context.DeltaLink;

                    long? countValue = writeContext.InternalRequest.Context.TotalCount;
                    if (countValue.HasValue)
                    {
                        resourceSet.Count = countValue.Value;
                    }
                }
            }
            else
            {
                // nested resourceSet
                ITruncatedCollection truncatedCollection = resourceSetInstance as ITruncatedCollection;
                if (truncatedCollection != null && truncatedCollection.IsTruncated)
                {
                    resourceSet.NextPageLink = GetNestedNextPageLink(writeContext, truncatedCollection.PageSize);
                }

                ICountOptionCollection countOptionCollection = resourceSetInstance as ICountOptionCollection;
                if (countOptionCollection != null && countOptionCollection.TotalCount != null)
                {
                    resourceSet.Count = countOptionCollection.TotalCount;
                }
            }

            return resourceSet;
        }

        /// <summary>
        /// Create the <see cref="ODataPagedResourceSet"/> to be written for the given resourceSet instance.
        /// </summary>
        /// <param name="resourceSetInstance">The instance representing the resourceSet being written.</param>
        /// <param name="resourceSetType">The EDM type of the resourceSet being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataResourceSet"/> object.</returns>
        internal ODataPagedResourceSet CreatePagedResourceSet(IEnumerable resourceSetInstance, IEdmCollectionTypeReference resourceSetType,
            ODataSerializerContext writeContext)
        {
            ODataPagedResourceSet pagedResourceSet = new ODataPagedResourceSet(); 
            ODataResourceSet resourceSet = new ODataResourceSet
            {
                TypeName = resourceSetType.FullName()
            };
            pagedResourceSet.ResourceSet = resourceSet;

            IEdmStructuredTypeReference structuredType = GetResourceType(resourceSetType).AsStructured();
            if (writeContext.NavigationSource != null && structuredType.IsEntity())
            {
                ResourceSetContext resourceSetContext = ResourceSetContext.Create(writeContext, resourceSetInstance);
                IEdmEntityType entityType = structuredType.AsEntity().EntityDefinition();
                var operations = writeContext.Model.GetAvailableOperationsBoundToCollection(entityType);
                var odataOperations = CreateODataOperations(operations, resourceSetContext, writeContext);
                foreach (var odataOperation in odataOperations)
                {
                    ODataAction action = odataOperation as ODataAction;
                    if (action != null)
                    {
                        resourceSet.AddAction(action);
                    }
                    else
                    {
                        resourceSet.AddFunction((ODataFunction)odataOperation);
                    }
                }
            }

            if (writeContext.ExpandedResource == null)
            {
                // If we have more OData format specific information apply it now, only if we are the root feed.
                PageResult odataResourceSetAnnotations = resourceSetInstance as PageResult;
                if (odataResourceSetAnnotations != null)
                {
                    resourceSet.Count = odataResourceSetAnnotations.Count;
                    resourceSet.NextPageLink = odataResourceSetAnnotations.NextPageLink;
                }
                else if (writeContext.Request != null)
                {
                    if (writeContext.InternalRequest.Context.NextLink != null)
                    {
                        resourceSet.NextPageLink = writeContext.InternalRequest.Context.NextLink;
                    }
                    else if (writeContext.InternalRequest.Context.PageSize > 0)
                    {
                        SkipTokenHandler skipTokenHandler = SkipTokenQueryOption.GetSkipTokenImplementation(writeContext.InternalRequest.Context.QueryOptions.Context);
                        pagedResourceSet.NextLinkFunction = (obj) => { return skipTokenHandler.GenerateNextPageLink(obj, writeContext); };
                    }
                    resourceSet.DeltaLink = writeContext.InternalRequest.Context.DeltaLink;

                    long? countValue = writeContext.InternalRequest.Context.TotalCount;
                    if (countValue.HasValue)
                    {
                        resourceSet.Count = countValue.Value;
                    }
                }
            }
            else
            {
                // nested resourceSet
                ITruncatedCollection truncatedCollection = resourceSetInstance as ITruncatedCollection;
                if (truncatedCollection != null && truncatedCollection.IsTruncated)
                {
                   pagedResourceSet.NextLinkFunction = (obj) => { return GetNestedNextPageLink(writeContext, truncatedCollection.PageSize, obj); };
                }

                ICountOptionCollection countOptionCollection = resourceSetInstance as ICountOptionCollection;
                if (countOptionCollection != null && countOptionCollection.TotalCount != null)
                {
                    resourceSet.Count = countOptionCollection.TotalCount;
                }
            }
            return pagedResourceSet;
        }

        /// <summary>
        ///  Creates an <see cref="ODataOperation" /> to be written for the given operation and the resourceSet instance.
        /// </summary>
        /// <param name="operation">The OData operation.</param>
        /// <param name="resourceSetContext">The context for the resourceSet instance being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created operation or null if the operation should not be written.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234: Pass System.Uri objects instead of strings", Justification = "This overload is equally good")]
        public virtual ODataOperation CreateODataOperation(IEdmOperation operation, ResourceSetContext resourceSetContext, ODataSerializerContext writeContext)
        {
            if (operation == null)
            {
                throw Error.ArgumentNull("operation");
            }

            if (resourceSetContext == null)
            {
                throw Error.ArgumentNull("resourceSetContext");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
            }

            ODataMetadataLevel metadataLevel = writeContext.MetadataLevel;
            IEdmModel model = writeContext.Model;

            if (metadataLevel != ODataMetadataLevel.FullMetadata)
            {
                return null;
            }

            OperationLinkBuilder builder;
            builder = model.GetOperationLinkBuilder(operation);

            if (builder == null)
            {
                return null;
            }

            Uri target = builder.BuildLink(resourceSetContext);
            if (target == null)
            {
                return null;
            }

            Uri baseUri = new Uri(writeContext.InternalUrlHelper.CreateODataLink(MetadataSegment.Instance));
            Uri metadata = new Uri(baseUri, "#" + operation.FullName());

            ODataOperation odataOperation;
            IEdmAction action = operation as IEdmAction;
            if (action != null)
            {
                odataOperation = new ODataAction();
            }
            else
            {
                odataOperation = new ODataFunction();
            }
            odataOperation.Metadata = metadata;

            // Always omit the title in minimal/no metadata modes.
            ODataResourceSerializer.EmitTitle(model, operation, odataOperation);

            // Omit the target in minimal/no metadata modes unless it doesn't follow conventions.
            if (metadataLevel == ODataMetadataLevel.FullMetadata || !builder.FollowsConventions)
            {
                odataOperation.Target = target;
            }

            return odataOperation;
        }

        private IEnumerable<ODataOperation> CreateODataOperations(IEnumerable<IEdmOperation> operations, ResourceSetContext resourceSetContext, ODataSerializerContext writeContext)
        {
            Contract.Assert(operations != null);
            Contract.Assert(resourceSetContext != null);
            Contract.Assert(writeContext != null);

            foreach (IEdmOperation operation in operations)
            {
                ODataOperation oDataOperation = CreateODataOperation(operation, resourceSetContext, writeContext);
                if (oDataOperation != null)
                {
                    yield return oDataOperation;
                }
            }
        }

        private static Uri GetNestedNextPageLink(ODataSerializerContext writeContext, int pageSize, object obj = null)
        {
            Contract.Assert(writeContext.ExpandedResource != null);
            IEdmNavigationSource sourceNavigationSource = writeContext.ExpandedResource.NavigationSource;
            NavigationSourceLinkBuilderAnnotation linkBuilder = writeContext.Model.GetNavigationSourceLinkBuilder(sourceNavigationSource);
            Uri navigationLink =
                linkBuilder.BuildNavigationLink(writeContext.ExpandedResource, writeContext.NavigationProperty);
            IList<OrderByNode> orderByNodes = null;
            Uri nestedNextLink = GenerateQueryFromExpandedItem(writeContext, navigationLink, out orderByNodes);
            SkipTokenHandler handler = SkipTokenQueryOption.GetSkipTokenImplementation(writeContext.QueryOptions.Context);
            Func<object, string> skipTokenGenerator = (member) => { return handler.GenerateSkipTokenValue(member, writeContext.Model, orderByNodes);  };
            if (nestedNextLink != null)
            {
                return GetNextPageHelper.GetNextPageLink(nestedNextLink, pageSize, obj, skipTokenGenerator);
            }

            return null;
        }

        private static Uri GenerateQueryFromExpandedItem(ODataSerializerContext writeContext, Uri navigationLink, out IList<OrderByNode> orderByNodes)
        {
            IWebApiUrlHelper urlHelper = writeContext.InternalUrlHelper;
            string serviceRoot = urlHelper.CreateODataLink(
                writeContext.InternalRequest.Context.RouteName,
                writeContext.InternalRequest.PathHandler,
                new List<ODataPathSegment>());
            Uri serviceRootUri = new Uri(serviceRoot);
            ODataUriParser parser = new ODataUriParser(writeContext.Model, serviceRootUri, navigationLink);
            ODataUri newUri = parser.ParseUri();
            newUri.SelectAndExpand = writeContext.SelectExpandClause;
            if (writeContext.ExpandedNavigationSelectItem != null)
            {
                newUri.OrderBy = writeContext.ExpandedNavigationSelectItem.OrderByOption;
                newUri.Filter = writeContext.ExpandedNavigationSelectItem.FilterOption;
                newUri.Skip = writeContext.ExpandedNavigationSelectItem.SkipOption;
                newUri.Top = writeContext.ExpandedNavigationSelectItem.TopOption;
            }

            if (newUri.OrderBy != null)
            {
                orderByNodes = OrderByNode.CreateCollection(newUri.OrderBy);
            }
            else
            {
                orderByNodes = null;
            }
            
            return newUri.BuildUri(ODataUrlKeyDelimiter.Slash);
        }

        private static IEdmStructuredTypeReference GetResourceType(IEdmTypeReference resourceSetType)
        {
            if (resourceSetType.IsCollection())
            {
                IEdmTypeReference elementType = resourceSetType.AsCollection().ElementType();
                if (elementType.IsEntity() || elementType.IsComplex())
                {
                    return elementType.AsStructured();
                }
            }

            string message = Error.Format(SRResources.CannotWriteType, typeof(ODataResourceSetSerializer).Name, resourceSetType.FullName());
            throw new SerializationException(message);
        }
    }

    /// <summary>
    /// Wrapper around ODataResourceSet to include NextLinkFunction untill we can enhance OData class.
    /// </summary>
    internal class ODataPagedResourceSet
    {
        public Func<object, Uri> NextLinkFunction { get; set; }

        public ODataResourceSet ResourceSet { get; set; }
    }
}
