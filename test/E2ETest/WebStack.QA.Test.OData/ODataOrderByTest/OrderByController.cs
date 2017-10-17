using System.Linq;
using System.Web.Http;
using System.Web.OData;

namespace WebStack.QA.Test.OData.ODataOrderByTest
{
    public class ItemsController : ODataController
    {
        private static readonly OrderByEdmModel.OrderByContext Db = new OrderByEdmModel.OrderByContext();

        static ItemsController()
        {
            if (Db.Items.Any())
            {
                return;
            }

            Db.Items.Add(new Item() { A = 1, C = 1, B = 99, Name = "#1 - A1 C1 B99" });
            Db.Items.Add(new Item() { A = 1, C = 2, B = 98, Name = "#2 - A1 C2 B98" });
            Db.Items.Add(new Item() { A = 1, C = 3, B = 97, Name = "#3 - A1 C3 B97" });
            Db.Items.Add(new Item() { A = 1, C = 4, B = 96, Name = "#4 - A1 C4 B96" });

            Db.SaveChanges();
        }

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(Db.Items);
        }        
    }
}