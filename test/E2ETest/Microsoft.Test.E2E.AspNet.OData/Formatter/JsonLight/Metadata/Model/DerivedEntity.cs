//-----------------------------------------------------------------------------
// <copyright file="DerivedEntity.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model
{
    public class DerivedEntity : BaseEntity
    {
        public DerivedEntity()
        {
        }

        public DerivedEntity(int id)
            : base(id)
        {
        }
    }
}
