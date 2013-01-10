// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Diagnostics.Contracts;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    internal class ODataFeedDeserializer : ODataEntryDeserializer<ODataFeed>
    {
        private IEdmCollectionTypeReference _edmCollectionType;
        private IEdmEntityTypeReference _edmEntityType;

        public ODataFeedDeserializer(IEdmCollectionTypeReference edmCollectionType, ODataDeserializerProvider deserializerProvider)
            : base(edmCollectionType, ODataPayloadKind.Feed, deserializerProvider)
        {
            _edmCollectionType = edmCollectionType;
            if (!edmCollectionType.ElementType().IsEntity())
            {
                throw Error.NotSupported(SRResources.TypeMustBeEntityCollection, edmCollectionType.ElementType().FullName(), typeof(IEdmEntityType).Name);
            }
            _edmEntityType = _edmCollectionType.ElementType().AsEntity();
        }

        public override object ReadInline(ODataFeed feed, ODataDeserializerContext readContext)
        {
            if (readContext == null)
            {
                throw Error.ArgumentNull("readContext");
            }

            if (feed == null)
            {
                return null;
            }

            // Recursion guard to avoid stack overflows
            EnsureStackHelper.EnsureStack();

            return ReadItems(feed, readContext);
        }

        private IEnumerable ReadItems(ODataFeed feed, ODataDeserializerContext readContext)
        {
            ODataEntryDeserializer deserializer = DeserializerProvider.GetODataDeserializer(_edmEntityType);

            ODataFeedAnnotation feedAnnotation = feed.GetAnnotation<ODataFeedAnnotation>();
            Contract.Assert(feedAnnotation != null, "Each feed we create should gave annotation on it.");

            foreach (ODataEntry entry in feedAnnotation)
            {
                ODataEntryAnnotation annotation = entry.GetAnnotation<ODataEntryAnnotation>();
                Contract.Assert(annotation != null);

                yield return deserializer.ReadInline(entry, readContext);
            }
        }
    }
}
