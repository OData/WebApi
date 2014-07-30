// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Properties;
using System.Web.Http.OData.Query;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;

namespace System.Web.Http.OData.Formatter.Serialization
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
            : base(ODataPayloadKind.Feed, serializerProvider)
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

            IEdmEntitySet entitySet = writeContext.EntitySet;
            if (entitySet == null)
            {
                throw new SerializationException(SRResources.EntitySetMissingDuringSerialization);
            }

            IEdmTypeReference feedType = writeContext.GetEdmType(graph, type);
            Contract.Assert(feedType != null);

            IEdmEntityTypeReference entityType = GetEntityType(feedType);
            ODataWriter writer = messageWriter.CreateODataFeedWriter(entitySet, entityType.EntityDefinition());
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
            ODataFeed feed = CreateODataFeed(enumerable, feedType.AsCollection(), writeContext);
            if (feed == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Feed));
            }

            ODataEdmTypeSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType);
            if (entrySerializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName(), typeof(ODataMediaTypeFormatter).Name));
            }

            // save this for later to support JSON lite streaming.
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
            // the next page link will be written early in a manner not compatible with streaming=true. Instead, if
            // the next page link is not set when calling WriteStart(feed) but is instead set later on that feed
            // object before calling WriteEnd(), the next page link will be written at the end, as required for
            // streaming=true support.

            if (nextPageLink != null)
            {
                feed.NextPageLink = nextPageLink;
            }

            writer.WriteEnd();
        }

        /// <summary>
        /// Create the <see cref="ODataFeed"/> to be written for the given feed instance.
        /// </summary>
        /// <param name="feedInstance">The instance representing the feed being written.</param>
        /// <param name="feedType">The EDM type of the feed being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataFeed"/> object.</returns>
        public virtual ODataFeed CreateODataFeed(IEnumerable feedInstance, IEdmCollectionTypeReference feedType,
            ODataSerializerContext writeContext)
        {
            ODataFeed feed = new ODataFeed();

            if (writeContext.EntitySet != null)
            {
                IEdmModel model = writeContext.Model;
                EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(writeContext.EntitySet);
                FeedContext feedContext = new FeedContext
                {
                    Request = writeContext.Request,
                    RequestContext = writeContext.RequestContext,
                    EntitySet = writeContext.EntitySet,
                    Url = writeContext.Url,
                    FeedInstance = feedInstance
                };

                Uri feedSelfLink = linkBuilder.BuildFeedSelfLink(feedContext);
                if (feedSelfLink != null)
                {
                    feed.SetAnnotation(new AtomFeedMetadata() { SelfLink = new AtomLinkMetadata() { Relation = "self", Href = feedSelfLink } });
                }
            }

            // TODO: Bug 467590: remove the hardcoded feed id. Get support for it from the model builder ?
            feed.Id = "http://schemas.datacontract.org/2004/07/" + feedType.FullName();

            if (writeContext.ExpandedEntity == null)
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

                    long? inlineCount = writeContext.Request.ODataProperties().TotalCount;
                    if (inlineCount.HasValue)
                    {
                        feed.Count = inlineCount.Value;
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
            }

            return feed;
        }

        private static Uri GetNestedNextPageLink(ODataSerializerContext writeContext, int pageSize)
        {
            Contract.Assert(writeContext.ExpandedEntity != null);

            IEdmEntitySet sourceEntitySet = writeContext.ExpandedEntity.EntitySet;
            EntitySetLinkBuilderAnnotation linkBuilder = writeContext.Model.GetEntitySetLinkBuilder(sourceEntitySet);
            Uri navigationLink =
                linkBuilder.BuildNavigationLink(writeContext.ExpandedEntity, writeContext.NavigationProperty, ODataMetadataLevel.Default);

            if (navigationLink != null)
            {
                return ODataQueryOptions.GetNextPageLink(navigationLink, pageSize);
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
