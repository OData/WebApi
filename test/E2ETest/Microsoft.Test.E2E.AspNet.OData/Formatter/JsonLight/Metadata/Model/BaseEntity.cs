//-----------------------------------------------------------------------------
// <copyright file="BaseEntity.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model
{
    public class BaseEntity
    {
        public BaseEntity()
        {
        }

        public BaseEntity(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        internal class IdEqualityComparer : IEqualityComparer<BaseEntity>
        {
            public bool Equals(BaseEntity x, BaseEntity y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(BaseEntity obj)
            {
                return obj.Id;
            }
        }
    }
}
