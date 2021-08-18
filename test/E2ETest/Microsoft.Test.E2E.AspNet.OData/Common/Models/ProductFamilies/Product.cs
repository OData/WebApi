//-----------------------------------------------------------------------------
// <copyright file="Product.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Models.ProductFamilies
{
    public partial class Product
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public DateTimeOffset? ReleaseDate { get; set; }

        public DateTimeOffset? SupportedUntil { get; set; }

        public virtual ProductFamily Family { get; set; }
    }
}
