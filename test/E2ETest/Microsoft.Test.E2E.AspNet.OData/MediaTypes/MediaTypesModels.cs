//-----------------------------------------------------------------------------
// <copyright file="MediaTypesModels.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Test.E2E.AspNet.OData.MediaTypes
{
    public class MediaTypesOrder
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public long TrackingNumber { get; set; }
    }
}
