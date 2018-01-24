// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Controllers
{
    public class EntityWithComplexPropertiesController : InMemoryODataController<EntityWithComplexProperties, int>
    {
        public EntityWithComplexPropertiesController()
            : base("Id")
        {
            var entities = MetadataTestHelpers.CreateInstances<EntityWithComplexProperties[]>();
            foreach (var entity in entities)
            {
                LocalTable.AddOrUpdate(entity.Id, entity, (key, oldEntity) => oldEntity);
            }
        }

        public override System.Threading.Tasks.Task<IEnumerable<EntityWithComplexProperties>> Get()
        {
            return base.Get();
        }
    }
}
