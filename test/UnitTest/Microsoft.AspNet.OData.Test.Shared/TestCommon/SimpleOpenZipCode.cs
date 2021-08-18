//-----------------------------------------------------------------------------
// <copyright file="SimpleOpenZipCode.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.AspNet.OData.Test.Common
{
    public class SimpleOpenZipCode
    {
        public int Code { get; set; }
        public IDictionary<string, object> Properties { get; set; }
    }
}
