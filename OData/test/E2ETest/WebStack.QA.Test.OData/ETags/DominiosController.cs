using System.Web.Http;
using System.Web.OData;

namespace WebStack.QA.Test.OData.ETags
{
    public class DominiosController : ODataController
    {
        private ETagCurrencyTokenEfContext _db = new ETagCurrencyTokenEfContext();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(_db.Dominios);
        }
    }
}
