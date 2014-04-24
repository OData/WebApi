// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace System.Web.OData
{
    public class SimpleOpenAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public IDictionary<string, object> Properties { get; set; }
    }
}