//-----------------------------------------------------------------------------
// <copyright file="Product.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.Test.E2E.AspNet.OData.SxS2.ODataV3.Models
{
    public class Product
    {
        public int Id
        {
            get;
            set;
        }

        public string Title
        {
            get; set;
        }

        public DateTime ManufactureDateTime
        {
            get;
            set;
        }
    }
}
