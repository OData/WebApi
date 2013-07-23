// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData feeds.
    /// </summary>
    public class ODataFeedDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataFeedDeserializer"/> class.
        /// </summary>
        /// <param name="edmType">The entity collection type that this deserializer can read.</param>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataFeedDeserializer(IEdmCollectionTypeReference edmType, ODataDeserializerProvider deserializerProvider)
            : base(edmType, ODataPayloadKind.Feed, deserializerProvider)
        {
            CollectionType = edmType;
            if (!edmType.ElementType().IsEntity())
            {
                throw Error.Argument("edmType", SRResources.TypeMustBeEntityCollection, edmType.ElementType().FullName(), typeof(IEdmEntityType).Name);
            }

            EntityType = CollectionType.ElementType().AsEntity();
        }

        /// <summary>
        /// Gets the entity collection type that this deserializer can read.
        /// </summary>
        public IEdmCollectionTypeReference CollectionType { get; private set; }

        /// <summary>
        /// Gets the entity type of the feed.
        /// </summary>
        public IEdmEntityTypeReference EntityType { get; private set; }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            ODataFeedWithEntries feed = item as ODataFeedWithEntries;
            if (feed == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataFeedWithEntries).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            return ReadFeed(feed, readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="feed"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="feed">The feed to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <returns>The deserialized feed object.</returns>
        public virtual IEnumerable ReadFeed(ODataFeedWithEntries feed, ODataDeserializerContext readContext)
        {
            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(EntityType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, EntityType.FullName(), typeof(ODataMediaTypeFormatter).Name));
            }

            foreach (ODataEntryWithNavigationLinks entry in feed.Entries)
            {
                yield return deserializer.ReadInline(entry, readContext);
            }
        }
    }
}
