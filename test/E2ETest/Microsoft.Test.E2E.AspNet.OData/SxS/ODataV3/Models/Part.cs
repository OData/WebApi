//-----------------------------------------------------------------------------
// <copyright file="Part.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.SxS.ODataV3.Models
{
    public class Part
    {
        public int PartId
        {
            get;
            set;
        }

        public DateTime ReleaseDateTime
        {
            get;
            set;
        }

        public virtual ICollection<Product> Products
        {
            get;
            set;
        }
    }
}
