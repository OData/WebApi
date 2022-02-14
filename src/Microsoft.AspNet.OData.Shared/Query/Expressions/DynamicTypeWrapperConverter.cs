//-----------------------------------------------------------------------------
// <copyright file="DynamicTypeWrapperConverter.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using Microsoft.AspNet.OData.Common;
using Newtonsoft.Json;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// Represents a custom <see cref="JsonConverter"/> to serialize <see cref="DynamicTypeWrapper"/> instances to JSON.
    /// </summary>
    internal class DynamicTypeWrapperConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == null)
            {
                throw Error.ArgumentNull("objectType");
            }

            return objectType.IsAssignableFrom(typeof(DynamicTypeWrapper));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Contract.Assert(false, "DynamicTypeWrapper is internal and should never be deserialized into.");
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DynamicTypeWrapper dynamicTypeWrapper = value as DynamicTypeWrapper;
            if (dynamicTypeWrapper != null)
            {
                serializer.Serialize(writer, dynamicTypeWrapper.Values);
            }
        }
    }
}
