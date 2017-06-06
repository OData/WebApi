using System.Collections.Generic;
using WebStack.QA.Test.OData.Common.Controllers;
using WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model;

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Controllers
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
