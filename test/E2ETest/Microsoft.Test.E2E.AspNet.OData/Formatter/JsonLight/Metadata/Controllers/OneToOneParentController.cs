//-----------------------------------------------------------------------------
// <copyright file="OneToOneParentController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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

#if NETCORE
        public ITestActionResult GetChild(int key)
        {
            return Ok(LocalTable[key].Child);
        }
#else
        public HttpResponseMessage GetChild(int key)
        {
            return Request.CreateResponse(HttpStatusCode.OK, LocalTable[key].Child);
        }
#endif
    }
}
