using System.Web.Http;
using WebStack.QA.Share.Controllers.TypeLibrary;

namespace WebStack.QA.Share.Controllers
{
    public class NorthwindController : ApiController
    {
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Customer EchoCustomer(Customer input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Order EchoOrder(Order input)
        {
            return input;
        }
    }
}
