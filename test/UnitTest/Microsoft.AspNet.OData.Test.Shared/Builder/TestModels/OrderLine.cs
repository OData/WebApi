//-----------------------------------------------------------------------------
// <copyright file="OrderLine.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class OrderLine
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public int OrderId { get; set; }
    }
}
