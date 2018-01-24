// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Controllers
{
    public class OneToOneChildController : InMemoryODataController<OneToOneChild, int>
    {
        public OneToOneChildController()
            : base("Id")
        {
            var entities = MetadataTestHelpers.CreateInstances<OneToOneParent[]>();
            foreach (var entity in entities.Select(x => x.Child))
            {
                LocalTable.AddOrUpdate(entity.Id, entity, (key, oldEntity) => oldEntity);
            }
        }
    }
}
