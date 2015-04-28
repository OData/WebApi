using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.OData;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Controllers
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

        public HttpResponseMessage Paged()
        {
            return Request.CreateResponse(HttpStatusCode.OK, new PageResult<StubEntity>(LocalTable.Values, new Uri("http://differentServer:5000/StubEntity/Default.Paged?$skip=" + LocalTable.Values.Count), LocalTable.Values.Count));
        }
    }
}
