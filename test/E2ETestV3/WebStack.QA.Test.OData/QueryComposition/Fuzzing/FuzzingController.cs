using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData;

namespace WebStack.QA.Test.OData.QueryComposition.Fuzzing
{
    public class FuzzingController : ApiController
    {
        private static EntityTypeModel1[] cachedEntities = null;

        static FuzzingController()
        {
            cachedEntities = FuzzingDataInitializer.Generate().ToArray();
        }

        [EnableQuery(PageSize = 999999)]
        public IEnumerable<EntityTypeModel1> Get()
        {
            return cachedEntities;
        }
    }

    public class FuzzingDbController : ApiController
    {
        private FuzzingContext context = new FuzzingContext();

        [EnableQuery]
        public IEnumerable<EntityTypeModel1> Get()
        {
            return context.EntityTypeModel1Set.AsEnumerable();
        }

        protected override void Dispose(bool disposing)
        {
            context.Dispose();
            base.Dispose(disposing);
        }
    }
}
