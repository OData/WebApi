// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Microsoft.AspNet.OData.Test.Common
{
    public class SimpleOpenCustomer
    {
        [Key]
        public int CustomerId { get; set; }
        public string Name { get; set; }
        public SimpleOpenAddress Address { get; set; }
        public string Website { get; set; }
        public List<SimpleOpenOrder> Orders { get; set; }
        public IDictionary<string, object> CustomerProperties { get; set; }
        public ODataInstanceAnnotationContainer InstanceAnnotations { get; set; }
    }
}