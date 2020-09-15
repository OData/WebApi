// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Common
{
    public class SimpleOpenAddress
    {
        public string Street { get; set; }
        public string City { get; set; }
        public IDictionary<string, object> Properties { get; set; }
        public ODataInstanceAnnotationContainer InstanceAnnotations { get; set; }
    }
}