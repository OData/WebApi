// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

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