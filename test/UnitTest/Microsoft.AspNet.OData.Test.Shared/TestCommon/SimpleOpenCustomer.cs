//-----------------------------------------------------------------------------
// <copyright file="SimpleOpenCustomer.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNet.OData.Builder;

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
