// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Controllers
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
