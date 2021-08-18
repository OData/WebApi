//-----------------------------------------------------------------------------
// <copyright file="SimpleOpenAddress.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Builder;

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
