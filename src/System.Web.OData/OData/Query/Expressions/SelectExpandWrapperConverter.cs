// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using Newtonsoft.Json;

namespace System.Web.Http.OData.Query.Expressions
{
    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="SelectExpandWrapper{TElement}"/> instances to JSON.
    /// </summary>
    internal class SelectExpandWrapperConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType.IsAssignableFrom(typeof(IDictionaryConvertible)))
            {
                return true;
            }
            return false;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Contract.Assert(false, "SelectExpandWrapper is internal and should never be deserialized into.");
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            IDictionaryConvertible dictionaryConvertible = value as IDictionaryConvertible;
            if (dictionaryConvertible != null)
            {
                serializer.Serialize(writer, dictionaryConvertible.ToDictionary());
            }
        }
    }
}
