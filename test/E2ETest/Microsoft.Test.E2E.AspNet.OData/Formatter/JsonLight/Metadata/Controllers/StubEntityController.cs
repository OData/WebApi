//-----------------------------------------------------------------------------
// <copyright file="StubEntityController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNet.OData;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Controllers
{
    public class StubEntityController : InMemoryODataController<StubEntity, int>
    {
        public StubEntityController()
            : base("Id")
        {
            var entities = MetadataTestHelpers.CreateInstances<StubEntity[]>();
            foreach (var entity in entities)
            {
                LocalTable.AddOrUpdate(entity.Id, entity, (key, oldEntity) => oldEntity);
            }
        }

        [EnableQuery(PageSize = 1)]
        public override Task<IEnumerable<StubEntity>> Get()
        {
            return base.Get();
        }

#if NETCORE
        public ITestActionResult Paged()
        {
            return Ok(new PageResult<StubEntity>(LocalTable.Values, new Uri("http://differentServer:5000/StubEntity/Default.Paged?$skip=" + LocalTable.Values.Count), LocalTable.Values.Count));
        }
#else
        public HttpResponseMessage Paged()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new PageResult<StubEntity>(LocalTable.Values, new Uri("http://differentServer:5000/StubEntity/Default.Paged?$skip=" + LocalTable.Values.Count), LocalTable.Values.Count));
        }
#endif
    }
}
