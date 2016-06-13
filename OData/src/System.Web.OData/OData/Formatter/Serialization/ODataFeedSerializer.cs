// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using System.Web.OData.Query;
using Microsoft.OData;
using Microsoft.OData.Edm;
using Microsoft.OData.UriParser;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" />
    /// </summary>
    public class ODataFeedSerializer : ODataEdmTypeSerializer
    {
        private const string Feed = "feed";

        /// <summary>
        /// Initializes a new instance of <see cref="ODataFeedSerializer"/>.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public ODataFeedSerializer(ODataSerializerProvider serializerProvider)
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

            IEdmTypeReference feedType = writeContext.GetEdmType(graph, type);
            Contract.Assert(feedType != null);

            IEdmEntityTypeReference entityType = GetEntityType(feedType);
            ODataWriter writer = messageWriter.CreateODataResourceSetWriter(entitySet, entityType.EntityDefinition());
            WriteObjectInline(graph, feedType, writer, writeContext);
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
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Feed));
            }

            IEnumerable enumerable = graph as IEnumerable; // Data to serialize
            if (enumerable == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            WriteFeed(enumerable, expectedType, writer, writeContext);
        }

        private void WriteFeed(IEnumerable enumerable, IEdmTypeReference feedType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(writer != null);
            Contract.Assert(writeContext != null);
            Contract.Assert(enumerable != null);
            Contract.Assert(feedType != null);

            IEdmEntityTypeReference elementType = GetEntityType(feedType);
            ODataResourceSet feed = CreateODataFeed(enumerable, feedType.AsCollection(), writeContext);
            if (feed == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Feed));
            }

            IEdmEntitySetBase entitySet = writeContext.NavigationSource as IEdmEntitySetBase;
            if (entitySet == null)
            {
                feed.SetSerializationInfo(new ODataResourceSerializationInfo
                {
                    IsFromCollection = true,
                    NavigationSourceEntityTypeName = elementType.FullName(),
                    NavigationSourceKind = EdmNavigationSourceKind.UnknownEntitySet,
                    NavigationSourceName = null
                });
            }

            ODataEdmTypeSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType);
            if (entrySerializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName(), typeof(ODataMediaTypeFormatter).Name));
            }

            // save this for later to support JSON odata.streaming.
            Uri nextPageLink = feed.NextPageLink;
            feed.NextPageLink = null;

            writer.WriteStart(feed);

            foreach (object entry in enumerable)
            {
                if (entry == null)
                {
                    throw new SerializationException(SRResources.NullElementInCollection);
                }

                entrySerializer.WriteObjectInline(entry, elementType, writer, writeContext);
            }

            // Subtle and suprising behavior: If the NextPageLink property is set before calling WriteStart(feed),
            // the next page link will be written early in a manner not compatible with odata.streaming=true. Instead, if
            // the next page link is not set when calling WriteStart(feed) but is instead set later on that feed
            // object before calling WriteEnd(), the next page link will be written at the end, as required for
            // odata.streaming=true support.

            if (nextPageLink != null)
            {
                feed.NextPageLink = nextPageLink;
            }

            writer.WriteEnd();
        }

        /// <summary>
        /// Create the <see cref="ODataResourceSet"/> to be written for the given feed instance.
        /// </summary>
        /// <param name="feedInstance">The instance representing the feed being written.</param>
        /// <param name="feedType">The EDM type of the feed being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataResourceSet"/> object.</returns>
        public virtual ODataResourceSet CreateODataFeed(IEnumerable feedInstance, IEdmCollectionTypeReference feedType,
            ODataSerializerContext writeContext)
        {
            ODataResourceSet feed = new ODataResourceSet();

            if (writeContext.NavigationSource != null)
            {
                FeedContext feedContext = new FeedContext
                {
                    Request = writeContext.Request,
                    RequestContext = writeContext.RequestContext,
                    EntitySetBase = writeContext.NavigationSource as IEdmEntitySetBase,
                    Url = writeContext.Url,
                    FeedInstance = feedInstance
                };

                IEdmEntityType entityType = GetEntityType(feedType).Definition as IEdmEntityType;
                var operations = writeContext.Model.GetAvailableOperationsBoundToCollection(entityType);
                var odataOperations = CreateODataOperations(operations, feedContext, writeContext);
                foreach (var odataOperation in odataOperations)
                {
                    ODataAction action = odataOperation as ODataAction;
                    if (action != null)
                    {
                        feed.AddAction(action);
                    }
                    else
                    {
                        feed.AddFunction((ODataFunction)odataOperation);
                    }
                }
            }

            if (writeContext.ExpandedResource == null)
            {
                // If we have more OData format specific information apply it now, only if we are the root feed.
                PageResult odataFeedAnnotations = feedInstance as PageResult;
                if (odataFeedAnnotations != null)
                {
                    feed.Count = odataFeedAnnotations.Count;
                    feed.NextPageLink = odataFeedAnnotations.NextPageLink;
                }
                else if (writeContext.Request != null)
                {
                    feed.NextPageLink = writeContext.Request.ODataProperties().NextLink;

                    long? countValue = writeContext.Request.ODataProperties().TotalCount;
                    if (countValue.HasValue)
                    {
                        feed.Count = countValue.Value;
                    }
                }
            }
            else
            {
                // nested feed
                ITruncatedCollection truncatedCollection = feedInstance as ITruncatedCollection;
                if (truncatedCollection != null && truncatedCollection.IsTruncated)
                {
                    feed.NextPageLink = GetNestedNextPageLink(writeContext, truncatedCollection.PageSize);
                }

                ICountOptionCollection countOptionCollection = feedInstance as ICountOptionCollection;
                if (countOptionCollection != null && countOptionCollection.TotalCount != null)
                {
                    feed.Count = countOptionCollection.TotalCount;
                }
            }

            return feed;
        }

        /// <summary>
        ///  Creates an <see cref="ODataOperation" /> to be written for the given operation and the feed instance.
        /// </summary>
        /// <param name="operation">The OData operation.</param>
        /// <param name="feedContext">The context for the feed instance being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created operation or null if the operation should not be written.</returns>
        [SuppressMessage("Microsoft.Usage", "CA2234: Pass System.Uri objects instead of strings", Justification = "This overload is equally good")]
        public virtual ODataOperation CreateODataOperation(IEdmOperation operation, FeedContext feedContext, ODataSerializerContext writeContext)
        {
            if (operation == null)
            {
                throw Error.ArgumentNull("operation");
            }

            if (feedContext == null)
            {
                throw Error.ArgumentNull("feedContext");
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

            Uri target = builder.BuildLink(feedContext);
            if (target == null)
            {
                return null;
            }

            Uri baseUri = new Uri(writeContext.Url.CreateODataLink(MetadataSegment.Instance));
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
            ODataEntityTypeSerializer.EmitTitle(model, operation, odataOperation);

            // Omit the target in minimal/no metadata modes unless it doesn't follow conventions.
            if (metadataLevel == ODataMetadataLevel.FullMetadata || !builder.FollowsConventions)
            {
                odataOperation.Target = target;
            }

            return odataOperation;
        }

        private IEnumerable<ODataOperation> CreateODataOperations(IEnumerable<IEdmOperation> operations, FeedContext feedContext, ODataSerializerContext writeContext)
        {
            Contract.Assert(operations != null);
            Contract.Assert(feedContext != null);
            Contract.Assert(writeContext != null);

            foreach (IEdmOperation operation in operations)
            {
                ODataOperation oDataOperation = CreateODataOperation(operation, feedContext, writeContext);
                if (oDataOperation != null)
                {
                    yield return oDataOperation;
                }
            }
        }

        private static Uri GetNestedNextPageLink(ODataSerializerContext writeContext, int pageSize)
        {
            Contract.Assert(writeContext.ExpandedResource != null);

            IEdmNavigationSource sourceNavigationSource = writeContext.ExpandedResource.NavigationSource;
            NavigationSourceLinkBuilderAnnotation linkBuilder = writeContext.Model.GetNavigationSourceLinkBuilder(sourceNavigationSource);
            Uri navigationLink =
                linkBuilder.BuildNavigationLink(writeContext.ExpandedResource, writeContext.NavigationProperty);

            if (navigationLink != null)
            {
                return HttpRequestMessageExtensions.GetNextPageLink(navigationLink, pageSize);
            }

            return null;
        }

        private static IEdmEntityTypeReference GetEntityType(IEdmTypeReference feedType)
        {
            if (feedType.IsCollection())
            {
                IEdmTypeReference elementType = feedType.AsCollection().ElementType();
                if (elementType.IsEntity())
                {
                    return elementType.AsEntity();
                }
            }

            string message = Error.Format(SRResources.CannotWriteType, typeof(ODataFeedSerializer).Name, feedType.FullName());
            throw new SerializationException(message);
        }
    }
}
