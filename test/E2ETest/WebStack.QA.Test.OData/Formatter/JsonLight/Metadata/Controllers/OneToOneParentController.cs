using System.Net;
using System.Net.Http;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Controllers
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
