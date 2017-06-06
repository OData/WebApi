using System.Linq;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Controllers
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
