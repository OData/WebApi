// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Net;
using System.Net.Http;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Controllers
{
    public class OneToOneParentController : InMemoryODataController<OneToOneParent, int>
    {
        public OneToOneParentController()
            : base("Id")
        {
            var entities = MetadataTestHelpers.CreateInstances<OneToOneParent[]>();
            foreach (var entity in entities)
            {
                LocalTable.AddOrUpdate(entity.Id, entity, (key, oldEntity) => oldEntity);
            }
        }

        public HttpResponseMessage GetChild(int key)
        {
            return Request.CreateResponse(HttpStatusCode.OK, LocalTable[key].Child);
        }
    }
}
