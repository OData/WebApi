//-----------------------------------------------------------------------------
// <copyright file="EntityWithComplexProperties.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model
{
    public class EntityWithComplexProperties
    {
        public int Id { get; set; }
        public List<string> StringListProperty { get; set; }
        public ComplexType ComplexProperty { get; set; }
    }
}
