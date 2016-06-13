// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using System.Web.OData.Query;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Serialization
{
    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" />
    /// The Collection is of <see cref="IEdmChangedObject"/> which is the base interface implemented by all objects which are a part of the DeltaFeed payload.
    /// </summary>
    public class ODataDeltaFeedSerializer : ODataEdmTypeSerializer
    {
        private const string DeltaFeed = "deltafeed";

        /// <summary>
        /// Initializes a new instance of <see cref="ODataDeltaFeedSerializer"/>.
        /// </summary>
        /// <param name="serializerProvider">The <see cref="ODataSerializerProvider"/> to use to write nested entries.</param>
        public ODataDeltaFeedSerializer(ODataSerializerProvider serializerProvider)
            : base(ODataPayloadKind.Delta, serializerProvider)
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

            if (graph == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaFeed));
            }

            IEdmEntitySetBase entitySet = writeContext.NavigationSource as IEdmEntitySetBase;
            if (entitySet == null)
            {
                throw new SerializationException(SRResources.EntitySetMissingDuringSerialization);
            }

            IEdmTypeReference feedType = writeContext.GetEdmType(graph, type);
            Contract.Assert(feedType != null);

            IEdmEntityTypeReference entityType = GetEntityType(feedType);
            ODataDeltaWriter writer = messageWriter.CreateODataDeltaWriter(entitySet, entityType.EntityDefinition());

            WriteDeltaFeedInline(graph, feedType, writer, writeContext);
        }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteDeltaFeedInline(object graph, IEdmTypeReference expectedType, ODataDeltaWriter writer,
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
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaFeed));
            }

            IEnumerable enumerable = graph as IEnumerable; // Data to serialize
            if (enumerable == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            WriteFeed(enumerable, expectedType, writer, writeContext);
        }

        private void WriteFeed(IEnumerable enumerable, IEdmTypeReference feedType, ODataDeltaWriter writer,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(writer != null);
            Contract.Assert(writeContext != null);
            Contract.Assert(enumerable != null);
            Contract.Assert(feedType != null);

            ODataDeltaResourceSet deltaFeed = CreateODataDeltaFeed(enumerable, feedType.AsCollection(), writeContext);
            if (deltaFeed == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaFeed));
            }

            // save this for later to support JSON odata.streaming.
            Uri nextPageLink = deltaFeed.NextPageLink;
            deltaFeed.NextPageLink = null;

            //Start writing of the Delta Feed
            writer.WriteStart(deltaFeed);

            //Iterate over all the entries present and select the appropriate write method.
            //Write method creates ODataDeltaDeletedEntry / ODataDeltaDeletedLink / ODataDeltaLink or ODataEntry.
            foreach (object entry in enumerable)
            {
                if (entry == null)
                {
                    throw new SerializationException(SRResources.NullElementInCollection);
                }

                IEdmChangedObject edmChangedObject = entry as IEdmChangedObject;
                if (edmChangedObject == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, enumerable.GetType().FullName));
                }

                switch (edmChangedObject.DeltaKind)
                {
                    case EdmDeltaEntityKind.DeletedEntry:
                        WriteDeltaDeletedEntry(entry, writer, writeContext);
                        break;
                    case EdmDeltaEntityKind.DeletedLinkEntry:
                        WriteDeltaDeletedLink(entry, writer, writeContext);
                        break;
                    case EdmDeltaEntityKind.LinkEntry:
                        WriteDeltaLink(entry, writer, writeContext);
                        break;
                    case EdmDeltaEntityKind.Entry:
                        {
                            IEdmEntityTypeReference elementType = GetEntityType(feedType);
                            ODataEntityTypeSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType) as ODataEntityTypeSerializer;
                            if (entrySerializer == null)
                            {
                                throw new SerializationException(
                                    Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName(), typeof(ODataMediaTypeFormatter).Name));
                            }
                            entrySerializer.WriteDeltaObjectInline(entry, elementType, writer, writeContext);
                            break;
                        }
                    default:
                        break;
                }
            }

            // Subtle and surprising behavior: If the NextPageLink property is set before calling WriteStart(feed),
            // the next page link will be written early in a manner not compatible with odata.streaming=true. Instead, if
            // the next page link is not set when calling WriteStart(feed) but is instead set later on that feed
            // object before calling WriteEnd(), the next page link will be written at the end, as required for
            // odata.streaming=true support.
            if (nextPageLink != null)
            {
                deltaFeed.NextPageLink = nextPageLink;
            }

            //End Writing of the Delta Feed
            writer.WriteEnd();
        }

        /// <summary>
        /// Create the <see cref="ODataDeltaResourceSet"/> to be written for the given feed instance.
        /// </summary>
        /// <param name="feedInstance">The instance representing the feed being written.</param>
        /// <param name="feedType">The EDM type of the feed being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The created <see cref="ODataDeltaResourceSet"/> object.</returns>
        public virtual ODataDeltaResourceSet CreateODataDeltaFeed(IEnumerable feedInstance, IEdmCollectionTypeReference feedType,
            ODataSerializerContext writeContext)
        {
            ODataDeltaResourceSet feed = new ODataDeltaResourceSet();

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
            }
            return feed;
        }

        /// <summary>
        /// Writes the given deltaDeletedEntry specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteDeltaDeletedEntry(object graph, ODataDeltaWriter writer, ODataSerializerContext writeContext)
        {
            EdmDeltaDeletedEntityObject edmDeltaDeletedEntity = graph as EdmDeltaDeletedEntityObject;
            if (edmDeltaDeletedEntity == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            ODataDeltaDeletedEntry deltaDeletedEntry = new ODataDeltaDeletedEntry(
               edmDeltaDeletedEntity.Id, edmDeltaDeletedEntity.Reason);

            if (deltaDeletedEntry != null)
            {
                writer.WriteDeltaDeletedEntry(deltaDeletedEntry);
            }
        }

        /// <summary>
        /// Writes the given deltaDeletedLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteDeltaDeletedLink(object graph, ODataDeltaWriter writer, ODataSerializerContext writeContext)
        {
            EdmDeltaDeletedLink edmDeltaDeletedLink = graph as EdmDeltaDeletedLink;
            if (edmDeltaDeletedLink == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            ODataDeltaDeletedLink deltaDeletedLink = new ODataDeltaDeletedLink(
                edmDeltaDeletedLink.Source,
                edmDeltaDeletedLink.Target,
                edmDeltaDeletedLink.Relationship);

            if (deltaDeletedLink != null)
            {
                writer.WriteDeltaDeletedLink(deltaDeletedLink);
            }
        }

        /// <summary>
        /// Writes the given deltaLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteDeltaLink(object graph, ODataDeltaWriter writer, ODataSerializerContext writeContext)
        {
            EdmDeltaLink edmDeltaLink = graph as EdmDeltaLink;
            if (edmDeltaLink == null)
            {
                throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
            }

            ODataDeltaLink deltaLink = new ODataDeltaLink(
                edmDeltaLink.Source,
                edmDeltaLink.Target,
                edmDeltaLink.Relationship);

            if (deltaLink != null)
            {
                writer.WriteDeltaLink(deltaLink);
            }
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
    }
}