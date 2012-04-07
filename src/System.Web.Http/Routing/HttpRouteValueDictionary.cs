// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace System.Web.Http.Routing
{
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "This class will never be serialized.")]
    public class HttpRouteValueDictionary : Dictionary<string, object>
    {
        public HttpRouteValueDictionary()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public HttpRouteValueDictionary(IDictionary<string, object> dictionary)
            : base(dictionary, StringComparer.OrdinalIgnoreCase)
        {
        }

        public HttpRouteValueDictionary(object values)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            if (values == null)
            {
                throw Error.ArgumentNull("values");
            }

            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(values);
            foreach (PropertyDescriptor prop in properties)
            {
                object val = prop.GetValue(values);
                Add(prop.Name, val);
            }
        }
    }
}
