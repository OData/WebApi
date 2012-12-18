// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Web.Http.OData.Builder;
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
    internal class ODataFeedSerializer : ODataEntrySerializer
    {
        private const string FeedNamespace = "http://schemas.datacontract.org/2004/07/";
        private const string SelfLinkRelation = "self";

        private readonly IEdmCollectionTypeReference _edmCollectionType;
        private readonly IEdmEntityType _edmElementType;

        public ODataFeedSerializer(IEdmCollectionTypeReference edmCollectionType, ODataSerializerProvider serializerProvider)
            : base(edmCollectionType, ODataPayloadKind.Feed, serializerProvider)
        {
            Contract.Assert(edmCollectionType != null);
            _edmCollectionType = edmCollectionType;
            if (!edmCollectionType.ElementType().IsEntity())
            {
                throw Error.NotSupported(SRResources.TypeMustBeEntityCollection, edmCollectionType.ElementType().FullName(), typeof(IEdmEntityType).Name);
            }

            Contract.Assert(edmCollectionType.ElementType() != null);
            Contract.Assert(edmCollectionType.ElementType().AsEntity() != null);
            Contract.Assert(edmCollectionType.ElementType().AsEntity().Definition != null);
            Contract.Assert(edmCollectionType.ElementType().AsEntity().Definition as IEdmEntityType != null);
            _edmElementType = _edmCollectionType.ElementType().AsEntity().Definition as IEdmEntityType;
        }

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

            // No null check; entity type is not required for successful serialization.
            IEdmEntityType entityType = _edmElementType;

            ODataWriter writer = messageWriter.CreateODataFeedWriter(entitySet, entityType);
            WriteObjectInline(graph, writer, writeContext);
            writer.Flush();
        }

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

            if (graph != null)
            {
                WriteFeed(graph, writer, writeContext);
            }
            else
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, ODataFormatterConstants.Feed));
            }
        }

         private void WriteFeed(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            IEnumerable enumerable = graph as IEnumerable; // Data to serialize
            if (enumerable != null)
            {
                ODataFeed feed = new ODataFeed();

                if (writeContext.EntitySet != null)
                {
                    EntitySetLinkBuilderAnnotation linkBuilder = SerializerProvider.EdmModel.GetEntitySetLinkBuilder(writeContext.EntitySet);
                    FeedContext feedContext = new FeedContext
                    {
                        EntitySet = writeContext.EntitySet,
                        UrlHelper = writeContext.UrlHelper,
                        PathHandler = writeContext.PathHandler,
                        FeedInstance = graph
                    };

                    Uri feedSelfLink = linkBuilder.BuildFeedSelfLink(feedContext);
                    if (feedSelfLink != null)
                    {
                        feed.SetAnnotation(new AtomFeedMetadata() { SelfLink = new AtomLinkMetadata() { Relation = SelfLinkRelation, Href = feedSelfLink } });
                    }
                }

                // TODO: Bug 467590: remove the hardcoded feed id. Get support for it from the model builder ?
                feed.Id = FeedNamespace + _edmCollectionType.FullName();

                // Compute and save the NextPageLink for JSON Light streaming support.
                Uri nextPageLink = null;

                // If we have more OData format specific information apply it now.
                ODataResult odataFeedAnnotations = graph as ODataResult;
                if (odataFeedAnnotations != null)
                {
                    feed.Count = odataFeedAnnotations.Count;
                    nextPageLink = odataFeedAnnotations.NextPageLink;
                }
                else
                {
                    HttpRequestMessage request = writeContext.Request;
                    if (request != null)
                    {
                        nextPageLink = request.GetNextPageLink();

                        long? inlineCount = request.GetInlineCount();
                        if (inlineCount.HasValue)
                        {
                            feed.Count = inlineCount.Value;
                        }
                    }
                }

                writer.WriteStart(feed);

                foreach (object entry in enumerable)
                {
                    if (entry == null)
                    {
                        throw Error.NotSupported(SRResources.NullElementInCollection);
                    }

                    ODataSerializer entrySerializer = SerializerProvider.GetODataPayloadSerializer(entry.GetType());
                    if (entrySerializer == null)
                    {
                        throw Error.NotSupported(SRResources.TypeCannotBeSerialized, entry.GetType(), typeof(ODataMediaTypeFormatter).Name);
                    }

                    Contract.Assert(entrySerializer.ODataPayloadKind == ODataPayloadKind.Entry);

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
        }
    }
}
