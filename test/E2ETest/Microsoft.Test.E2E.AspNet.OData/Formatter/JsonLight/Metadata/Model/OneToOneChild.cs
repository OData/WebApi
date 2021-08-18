//-----------------------------------------------------------------------------
// <copyright file="OneToOneChild.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model
{
    public class OneToOneChild
    {
        public int Id { get; set; }
        public OneToOneParent Parent { get; set; }
    }
}
