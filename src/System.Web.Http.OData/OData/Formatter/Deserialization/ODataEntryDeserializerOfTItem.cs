// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Base class for all <see cref="ODataDeserializer" />'s that deserialize into an object backed by <see cref="IEdmType"/>.
    /// </summary>
    /// <typeparam name="TItem">The item type that this deserializer understands.</typeparam>
    public abstract class ODataEntryDeserializer<TItem> : ODataEntryDeserializer
        where TItem : class
    {
        protected ODataEntryDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind)
            : base(edmType, payloadKind)
        {
        }

        protected ODataEntryDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind, ODataDeserializerProvider deserializerProvider)
            : base(edmType, payloadKind, deserializerProvider)
        {
        }

        public sealed override object ReadInline(object item, ODataDeserializerContext readContext)
        {
            if (item == null)
            {
                return null;
            }

            TItem typedItem = item as TItem;
            if (typedItem == null)
            {
                throw Error.Argument("item", SRResources.ArgumentMustBeOfType, typeof(TItem).Name);
            }

            return ReadInline(typedItem, readContext);
        }

        /// <summary>
        /// Deserializes the item into a new object of type corresponding to <see cref="P:EdmType"/>.
        /// </summary>
        /// <param name="item">The item to deserialize.</param>
        /// <param name="readContext">The <see cref="ODataDeserializerContext"/></param>
        /// <returns>The deserialized object.</returns>
        public virtual object ReadInline(TItem item, ODataDeserializerContext readContext)
        {
            throw Error.NotSupported(SRResources.DoesNotSupportReadInLine, GetType().Name);
        }
    }
}
