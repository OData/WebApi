// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;

namespace System.Web.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer"/> that can read OData feeds.
    /// </summary>
    public class ODataFeedDeserializer : ODataEdmTypeDeserializer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataFeedDeserializer"/> class.
        /// </summary>
        /// <param name="deserializerProvider">The deserializer provider to use to read inner objects.</param>
        public ODataFeedDeserializer(ODataDeserializerProvider deserializerProvider)
            : base(ODataPayloadKind.Feed, deserializerProvider)
        {
        }

        /// <inheritdoc />
        public sealed override object ReadInline(object item, IEdmTypeReference edmType, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }
            if (edmType == null)
            {
                throw Error.ArgumentNull("edmType");
            }
            if (!edmType.IsCollection() || !edmType.AsCollection().ElementType().IsEntity())
            {
                throw Error.Argument("edmType", SRResources.TypeMustBeEntityCollection, edmType.ToTraceString(), typeof(IEdmEntityType).Name);
            }

            IEdmEntityTypeReference elementType = edmType.AsCollection().ElementType().AsEntity();

            ODataFeedWithEntries feed = item as ODataFeedWithEntries;
            if (feed == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(ODataFeedWithEntries).Name);
            }

            // Recursion guard to avoid stack overflows
            RuntimeHelpers.EnsureSufficientExecutionStack();

            return ReadFeed(feed, elementType, readContext);
        }

        /// <summary>
        /// Deserializes the given <paramref name="feed"/> under the given <paramref name="readContext"/>.
        /// </summary>
        /// <param name="feed">The feed to deserialize.</param>
        /// <param name="readContext">The deserializer context.</param>
        /// <param name="elementType">The element type of the feed being read.</param>
        /// <returns>The deserialized feed object.</returns>
        public virtual IEnumerable ReadFeed(ODataFeedWithEntries feed, IEdmEntityTypeReference elementType, ODataDeserializerContext readContext)
        {
            ODataEdmTypeDeserializer deserializer = DeserializerProvider.GetEdmTypeDeserializer(elementType);
            if (deserializer == null)
            {
                throw new SerializationException(
                    Error.Format(SRResources.TypeCannotBeDeserialized, elementType.FullName(), typeof(ODataMediaTypeFormatter).Name));
            }

            foreach (ODataEntryWithNavigationLinks entry in feed.Entries)
            {
                yield return deserializer.ReadInline(entry, elementType, readContext);
            }
        }
    }
}
