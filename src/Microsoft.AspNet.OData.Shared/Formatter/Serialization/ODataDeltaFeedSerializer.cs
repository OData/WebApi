// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.OData;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Formatter.Serialization
{
    /// <summary>
    /// OData serializer for serializing a collection of <see cref="IEdmEntityType" />
    /// The Collection is of <see cref="IEdmChangedObject"/> which is the base interface implemented by all objects which are a part of the DeltaFeed payload.
    /// </summary>
    public class ODataDeltaFeedSerializer : ODataEdmTypeSerializer
    {
        private const string DeltaFeed = "deltafeed";
        IEdmStructuredTypeReference _elementType;

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

            IEdmEntityTypeReference entityType = GetResourceType(feedType).AsEntity();
            ODataWriter writer = messageWriter.CreateODataDeltaResourceSetWriter(entitySet, entityType.EntityDefinition());

            WriteDeltaFeedInline(graph, feedType, writer, writeContext);
        }

        /// <inheritdoc />
        public override async Task WriteObjectAsync(object graph, Type type, ODataMessageWriter messageWriter, ODataSerializerContext writeContext)
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

            IEdmEntityTypeReference entityType = GetResourceType(feedType).AsEntity();
            ODataWriter writer = await messageWriter.CreateODataDeltaResourceSetWriterAsync(entitySet, entityType.EntityDefinition());

            await WriteDeltaFeedInlineAsync(graph, feedType, writer, writeContext);
        }

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual void WriteDeltaFeedInline(object graph, IEdmTypeReference expectedType, ODataWriter writer,
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

        /// <summary>
        /// Writes the given object specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="expectedType">The expected EDM type of the object represented by <paramref name="graph"/>.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>
        public virtual async Task WriteDeltaFeedInlineAsync(object graph, IEdmTypeReference expectedType, ODataWriter writer,
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

            await WriteFeedAsync(enumerable, expectedType, writer, writeContext);
        }

        private void WriteFeed(IEnumerable enumerable, IEdmTypeReference feedType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(writer != null);
            Contract.Assert(writeContext != null);
            Contract.Assert(enumerable != null);
            Contract.Assert(feedType != null);

            IEdmStructuredTypeReference elementType = GetResourceType(feedType);
            _elementType = elementType;

            if (elementType.IsComplex())
            {
                ODataResourceSet resourceSet = new ODataResourceSet()
                {
                    TypeName = feedType.FullName()
                };

                writer.WriteStart(resourceSet);

                ODataResourceSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType) as ODataResourceSerializer;
                if (entrySerializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName()));
                }

                foreach (object entry in enumerable)
                {
                    entrySerializer.WriteDeltaObjectInline(entry, elementType, writer, writeContext);
                }
            }
            else
            {
                ODataDeltaResourceSet deltaFeed = CreateODataDeltaFeed(enumerable, feedType.AsCollection(), writeContext);
                if (deltaFeed == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaFeed));
                }

                // save the next page link for later to support JSON odata.streaming.
                Func<object, Uri> nextLinkGenerator = GetNextLinkGenerator(deltaFeed, enumerable, writeContext);
                deltaFeed.NextPageLink = null;

                //Start writing of the Delta Feed
                writer.WriteStart(deltaFeed);

                object lastResource = null;
                //Iterate over all the entries present and select the appropriate write method.
                //Write method creates ODataDeltaDeletedEntry / ODataDeltaDeletedLink / ODataDeltaLink or ODataEntry.
                foreach (object entry in enumerable)
                {
                    if (entry == null)
                    {
                        throw new SerializationException(SRResources.NullElementInCollection);
                    }

                    lastResource = entry;

                    EdmDeltaEntityKind deltaEntityKind;
                    if (writeContext.IsUntyped)
                    {
                        IEdmChangedObject edmChangedObject = entry as IEdmChangedObject;
                        if (edmChangedObject == null)
                        {
                            throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, enumerable.GetType().FullName));
                        }

                        deltaEntityKind = edmChangedObject.DeltaKind;
                    }
                    else
                    {
                        IDeltaSetItem deltaSetItem = entry as IDeltaSetItem;

                        if (deltaSetItem == null)
                        {
                            throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, enumerable.GetType().FullName));
                        }

                        deltaEntityKind = deltaSetItem.DeltaKind;
                    }
                                      
                    switch (deltaEntityKind)
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
                                ODataResourceSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType) as ODataResourceSerializer;

                                if (entrySerializer == null)
                                {
                                    throw new SerializationException(
                                        Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName()));
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

                deltaFeed.NextPageLink = nextLinkGenerator(lastResource);
            }

            //End Writing of the Delta Feed
            writer.WriteEnd();
        }

        private async Task WriteFeedAsync(IEnumerable enumerable, IEdmTypeReference feedType, ODataWriter writer,
            ODataSerializerContext writeContext)
        {
            Contract.Assert(writer != null);
            Contract.Assert(writeContext != null);
            Contract.Assert(enumerable != null);
            Contract.Assert(feedType != null);

            IEdmStructuredTypeReference elementType = GetResourceType(feedType);
            _elementType = elementType;

            if (elementType.IsComplex())
            {
                ODataResourceSet resourceSet = new ODataResourceSet()
                {
                    TypeName = feedType.FullName()
                };

                await writer.WriteStartAsync(resourceSet);

                ODataResourceSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType) as ODataResourceSerializer;
                if (entrySerializer == null)
                {
                    throw new SerializationException(
                        Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName()));
                }

                foreach (object entry in enumerable)
                {
                    await entrySerializer.WriteDeltaObjectInlineAsync(entry, elementType, writer, writeContext);
                }
            }
            else
            {
                ODataDeltaResourceSet deltaFeed = CreateODataDeltaFeed(enumerable, feedType.AsCollection(), writeContext);
                if (deltaFeed == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotSerializerNull, DeltaFeed));
                }

                // save the next page link for later to support JSON odata.streaming.
                Func<object, Uri> nextLinkGenerator = GetNextLinkGenerator(deltaFeed, enumerable, writeContext);
                deltaFeed.NextPageLink = null;

                //Start writing of the Delta Feed
                await writer.WriteStartAsync(deltaFeed);

                object lastResource = null;
                //Iterate over all the entries present and select the appropriate write method.
                //Write method creates ODataDeltaDeletedEntry / ODataDeltaDeletedLink / ODataDeltaLink or ODataEntry.
                foreach (object entry in enumerable)
                {
                    if (entry == null)
                    {
                        throw new SerializationException(SRResources.NullElementInCollection);
                    }

                    lastResource = entry;

                    EdmDeltaEntityKind deltaEntityKind;
                    if (writeContext.IsUntyped)
                    {
                        IEdmChangedObject edmChangedObject = entry as IEdmChangedObject;
                        if (edmChangedObject == null)
                        {
                            throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, enumerable.GetType().FullName));
                        }

                        deltaEntityKind = edmChangedObject.DeltaKind;
                    }
                    else
                    {
                        IDeltaSetItem deltaSetItem = entry as IDeltaSetItem;

                        if (deltaSetItem == null)
                        {
                            throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, enumerable.GetType().FullName));
                        }

                        deltaEntityKind = deltaSetItem.DeltaKind;
                    }
                                        

                    switch (deltaEntityKind)
                    {
                        case EdmDeltaEntityKind.DeletedEntry:
                            await WriteDeltaDeletedEntryAsync(entry, writer, writeContext);
                            break;
                        case EdmDeltaEntityKind.DeletedLinkEntry:
                            await WriteDeltaDeletedLinkAsync(entry, writer, writeContext);
                            break;
                        case EdmDeltaEntityKind.LinkEntry:
                            await WriteDeltaLinkAsync(entry, writer, writeContext);
                            break;
                        case EdmDeltaEntityKind.Entry:
                            {
                                ODataResourceSerializer entrySerializer = SerializerProvider.GetEdmTypeSerializer(elementType) as ODataResourceSerializer;
                                if (entrySerializer == null)
                                {
                                    throw new SerializationException(
                                        Error.Format(SRResources.TypeCannotBeSerialized, elementType.FullName()));
                                }
                                await entrySerializer.WriteDeltaObjectInlineAsync(entry, elementType, writer, writeContext);
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

                deltaFeed.NextPageLink = nextLinkGenerator(lastResource);
            }

            //End Writing of the Delta Feed
            await writer.WriteEndAsync();
        }

        /// <summary>
        /// Creates a function that takes in an object and generates nextlink uri.
        /// </summary>
        /// <param name="deltaFeed">The resource set describing a collection of structured objects.</param>
        /// <param name="enumerable">>The instance representing the resourceSet being written.</param>
        /// <param name="writeContext">The serializer context.</param>
        /// <returns>The function that generates the NextLink from an object.</returns>
        /// <returns></returns>
        internal static Func<object, Uri> GetNextLinkGenerator(ODataDeltaResourceSet deltaFeed, IEnumerable enumerable, ODataSerializerContext writeContext)
        {
            return ODataResourceSetSerializer.GetNextLinkGenerator(deltaFeed, enumerable, writeContext);
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
                    feed.NextPageLink = writeContext.InternalRequest.Context.NextLink;
                    feed.DeltaLink = writeContext.InternalRequest.Context.DeltaLink;

                    long? countValue = writeContext.InternalRequest.Context.TotalCount;
                    if (countValue.HasValue)
                    {
                        feed.Count = countValue.Value;
                    }
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
        public virtual void WriteDeltaDeletedEntry(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            ODataResourceSerializer serializer = SerializerProvider.GetEdmTypeSerializer(_elementType) as ODataResourceSerializer;
            ResourceContext resourceContext = serializer.GetResourceContext(graph, writeContext);
            SelectExpandNode selectExpandNode = serializer.CreateSelectExpandNode(resourceContext);
         
            if (selectExpandNode != null)
            {
                ODataDeletedResource deletedResource = GetDeletedResource(graph, resourceContext, serializer, selectExpandNode, writeContext.IsUntyped);                

                if (deletedResource != null)
                {
                    writer.WriteStart(deletedResource);
                    serializer.WriteDeltaComplexProperties(selectExpandNode, resourceContext, writer);
                    writer.WriteEnd();
                }
            }
        }

        /// <summary>
        /// Writes the given deltaDeletedEntry specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>        
        public virtual async Task WriteDeltaDeletedEntryAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            ODataResourceSerializer serializer = SerializerProvider.GetEdmTypeSerializer(_elementType) as ODataResourceSerializer;
            ResourceContext resourceContext = serializer.GetResourceContext(graph, writeContext);
            SelectExpandNode selectExpandNode = serializer.CreateSelectExpandNode(resourceContext);

            if (selectExpandNode != null)
            {
                ODataDeletedResource deletedResource = GetDeletedResource(graph, resourceContext, serializer, selectExpandNode, writeContext.IsUntyped);

                if (deletedResource != null)
                {
                    await writer.WriteStartAsync(deletedResource);
                    await writer.WriteEndAsync();
                }
            }
        }

        /// <summary>
        /// Writes the given deltaDeletedLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>        
        public virtual void WriteDeltaDeletedLink(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            ODataDeltaDeletedLink deltaDeletedLink = GetDeletedLink(graph);
            if (deltaDeletedLink != null)
            {
                writer.WriteDeltaDeletedLink(deltaDeletedLink);
            }
        }

        /// <summary>
        /// Writes the given deltaDeletedLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>        
        public virtual async Task WriteDeltaDeletedLinkAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            ODataDeltaDeletedLink deltaDeletedLink = GetDeletedLink(graph);
            if (deltaDeletedLink != null)
            {
                await writer.WriteDeltaDeletedLinkAsync(deltaDeletedLink);
            }
        }

        /// <summary>
        /// Writes the given deltaLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>        
        public virtual void WriteDeltaLink(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            ODataDeltaLink deltaLink = GetDeltaLink(graph);
            if (deltaLink != null)
            {
                writer.WriteDeltaLink(deltaLink);
            }
        }

        /// <summary>
        /// Writes the given deltaLink specified by the parameter graph as a part of an existing OData message using the given
        /// messageWriter and the writeContext.
        /// </summary>
        /// <param name="graph">The object to be written.</param>
        /// <param name="writer">The <see cref="ODataDeltaWriter" /> to be used for writing.</param>
        /// <param name="writeContext">The <see cref="ODataSerializerContext"/>.</param>        
        public async Task WriteDeltaLinkAsync(object graph, ODataWriter writer, ODataSerializerContext writeContext)
        {
            ODataDeltaLink deltaLink = GetDeltaLink(graph);
            if (deltaLink != null)
            {
                await writer.WriteDeltaLinkAsync(deltaLink);
            }
        }

  
        private ODataDeletedResource GetDeletedResource(object graph, ResourceContext resourceContext, ODataResourceSerializer serializer, SelectExpandNode selectExpandNode, bool isUntyped)
        {
            IEdmNavigationSource navigationSource;
            ODataDeletedResource deletedResource = serializer.CreateDeletedResource(selectExpandNode, resourceContext);

            if (isUntyped)
            {
                EdmDeltaDeletedEntityObject edmDeltaDeletedEntity = graph as EdmDeltaDeletedEntityObject;
                if (edmDeltaDeletedEntity == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
                }

                deletedResource.Id = StringToUri(edmDeltaDeletedEntity.Id??string.Empty);
                deletedResource.Reason = edmDeltaDeletedEntity.Reason;
                navigationSource = edmDeltaDeletedEntity.NavigationSource;
            }
            else
            {
                IDeltaDeletedEntityObject deltaDeletedEntity = graph as IDeltaDeletedEntityObject;
                if (deltaDeletedEntity == null)
                {
                    throw new SerializationException(Error.Format(SRResources.CannotWriteType, GetType().Name, graph.GetType().FullName));
                }

                deletedResource.Id = deltaDeletedEntity.Id;
                deletedResource.Reason = deltaDeletedEntity.Reason;
                navigationSource = deltaDeletedEntity.NavigationSource;
            }
            
            if (navigationSource != null)
            {
                ODataResourceSerializationInfo serializationInfo = new ODataResourceSerializationInfo
                {
                    NavigationSourceName = navigationSource.Name
                };
                deletedResource.SetSerializationInfo(serializationInfo);
            }
            
            return deletedResource;
        }

        private ODataDeltaDeletedLink GetDeletedLink(object graph)
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

            return deltaDeletedLink;
        }

        private ODataDeltaLink GetDeltaLink(object graph)
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

            return deltaLink;
        }

        private static IEdmStructuredTypeReference GetResourceType(IEdmTypeReference feedType)
        {
            if (feedType.IsCollection())
            {
                IEdmTypeReference elementType = feedType.AsCollection().ElementType();
                if (elementType.IsEntity() || elementType.IsComplex())
                {
                    return elementType.AsStructured();
                }
            }

            string message = Error.Format(SRResources.CannotWriteType, typeof(ODataResourceSetSerializer).Name, feedType.FullName());
            throw new SerializationException(message);
        }

        /// <summary>
        /// Safely returns the specified string as a relative or absolute Uri.
        /// </summary>
        /// <param name="uriString">The string to convert to a Uri.</param>
        /// <returns>The string as a Uri.</returns>
        internal static Uri StringToUri(string uriString)
        {
            Uri uri = null;
            try
            {
                uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
            }
            catch (FormatException)
            {
                // The Uri constructor throws a format exception if it can't figure out the type of Uri
                uri = new Uri(uriString, UriKind.Relative);
            }

            return uri;
        }
    }
}
