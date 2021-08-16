//-----------------------------------------------------------------------------
// <copyright file="ActionResultODataModel.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.ActionResult
{
    public class Customer
    {
        public string Id { get; set; }

        public IEnumerable<Book> Books { get; set; }
    }

    public class Book
    {
        public string Id { get; set; }
    }
}
