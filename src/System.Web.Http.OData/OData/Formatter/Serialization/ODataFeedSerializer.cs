// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Properties;
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
        /// <param name="edmType">The <see cref="IEdmCollectionTypeReference"/> that this serializer handles.</param>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public ODataFeedSerializer(IEdmCollectionTypeReference edmType, ODataSerializerProvider serializerProvider)
            : base(edmType, ODataPayloadKind.Feed, serializerProvider)
        {
            Contract.Assert(edmType != null);
            if (!edmType.ElementType().IsEntity())
            {
                throw Error.NotSupported(SRResources.TypeMustBeEntityCollection, edmType.ElementType().FullName(), typeof(IEdmEntityType).Name);
            }

            Contract.Assert(edmType.ElementType() != null);
            Contract.Assert(edmType.ElementType().AsEntity() != null);

            EntityCollectionType = edmType;
            EntityType = EntityCollectionType.ElementType().AsEntity();
        }

        /// <summary>
        /// Gets the <see cref="IEdmCollectionTypeReference"/> of the feed.
        /// </summary>
        public IEdmCollectionTypeReference EntityCollectionType { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEdmEntityType"/> of the entries of this feed.
        /// </summary>
        public IEdmEntityTypeReference EntityType { get; private set; }

        /// <inheritdoc />
        public override void WriteObject(object graph, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
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

            IEdmEntityType entityType = EntityType.EntityDefinition();

            ODataWriter writer = messageWriter.CreateODataFeedWriter(entitySet, entityType);
            WriteObjectInline(graph, writer, writeContext);
        }

        /// <inheritdoc />
        public override void WriteObjectInline(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            if (writer == null)
            {
                throw Error.ArgumentNull("writer");
            }

            if (writeContext == null)
            {
                throw Error.ArgumentNull("writeContext");
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

            WriteFeed(enumerable, writer, writeContext);
        }

        private void WriteFeed(IEnumerable enumerable, ODataWriter writer, ODataSerializerContext writeContext)
        {
            Contract.Assert(writer != null);
            Contract.Assert(writeContext != null);
            Contract.Assert(enumerable != null);

            ODataFeed feed = CreateODataFeed(enumerable, writeContext);
            if (feed == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, Feed));
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

                ODataEdmTypeSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(writeContext.Model, entry);
                if (entrySerializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeSerialized, entry.GetType(), typeof(ODataMediaTypeFormatter).Name));
                }

                entrySerializer.WriteObjectInline(entry, writer, writeContext);
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
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataFeed"/> object.</returns>
        public virtual ODataFeed CreateODataFeed(IEnumerable feedInstance, ODataSerializerContext writeContext)
        {
            ODataFeed feed = new ODataFeed();

            if (writeContext.EntitySet != null)
            {
                IEdmModel model = writeContext.Model;
                EntitySetLinkBuilderAnnotation linkBuilder = model.GetEntitySetLinkBuilder(writeContext.EntitySet);
                FeedContext feedContext = new FeedContext
                {
                    Request = writeContext.Request,
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
            feed.Id = "http://schemas.datacontract.org/2004/07/" + EntityCollectionType.FullName();

            // If we have more OData format specific information apply it now, only if we are the root feed.
            if (!writeContext.IsNested)
            {
                PageResult odataFeedAnnotations = feedInstance as PageResult;
                if (odataFeedAnnotations != null)
                {
                    feed.Count = odataFeedAnnotations.Count;
                    feed.NextPageLink = odataFeedAnnotations.NextPageLink;
                }
                else if (writeContext.Request != null)
                {
                    feed.NextPageLink = writeContext.Request.GetNextPageLink();

                    long? inlineCount = writeContext.Request.GetInlineCount();
                    if (inlineCount.HasValue)
                    {
                        feed.Count = inlineCount.Value;
                    }
                }
            }

            return feed;
        }
    }
}
