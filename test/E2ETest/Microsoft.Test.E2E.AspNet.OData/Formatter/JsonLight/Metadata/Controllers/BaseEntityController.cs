//-----------------------------------------------------------------------------
// <copyright file="BaseEntityController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Linq;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Controllers
{
    public class BaseEntityController : InMemoryODataController<BaseEntity, int>
    {
        public BaseEntityController()
            : base("Id")
        {
            BaseEntity[] baseEntities = new BaseEntity[] 
            { 
                new BaseEntity(1), new BaseEntity(2), new BaseEntity(3), 
                new BaseEntity(4), new BaseEntity(5), new BaseEntity(6), new BaseEntity(7) 
            };
            DerivedEntity[] derivedEntities = new DerivedEntity[] 
            { 
                new DerivedEntity(8), new DerivedEntity(9), new DerivedEntity(10), 
                new DerivedEntity(11), new DerivedEntity(12), new DerivedEntity(13), new DerivedEntity(14) 
            };
            foreach (var entity in baseEntities.Union(derivedEntities, new BaseEntity.IdEqualityComparer()))
            {
                LocalTable.AddOrUpdate(entity.Id, entity, (key, oldEntity) => oldEntity);
            }
        }

        public void AlwaysAvailableActionBaseType(int key)
        {
        }

        public void TransientActionBaseType(int key)
        {
        }

        public void AlwaysAvailableActionDerivedType(int key)
        {
        }

        public void TransientActionDerivedType(int key)
        {
        }
    }
}
