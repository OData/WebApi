// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter.Deserialization
{
    /// <summary>
    /// Represents an <see cref="ODataDeserializer" />'s that deserializes into an object backed by <see cref="IEdmType"/>.
    /// </summary>
    /// <typeparam name="TItem">The item type that this deserializer understands.</typeparam>
    public abstract class ODataEntryDeserializer<TItem> : ODataEntryDeserializer
        where TItem : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntryDeserializer{TItem}"/> class.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="payloadKind">The kind of OData payload this deserializer handles.</param>
        protected ODataEntryDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind)
            : base(edmType, payloadKind)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataEntryDeserializer"/> class.
        /// </summary>
        /// <param name="edmType">The EDM type.</param>
        /// <param name="payloadKind">The kind of OData payload this deserializer handles.</param>
        /// <param name="deserializerProvider">The <see cref="ODataDeserializerProvider"/>.</param>
        protected ODataEntryDeserializer(IEdmTypeReference edmType, ODataPayloadKind payloadKind, ODataDeserializerProvider deserializerProvider)
            : base(edmType, payloadKind, deserializerProvider)
        {
        }

        /// <inheritdoc />
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
