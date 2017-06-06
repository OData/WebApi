using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace WebStack.QA.Test.OData.Routing.DynamicProperties
{
    public class DynamicCustomersController : ODataController
    {
        public IHttpActionResult GetId(int key)
        {
            return Ok(string.Format("{0}_{1}", "Id", key));
        }

        public IHttpActionResult GetDynamicProperty(int key, string dynamicProperty)
        {
            return Ok(string.Format("{0}_{1}_{2}", dynamicProperty, "GetDynamicProperty", key));
        }

        public IHttpActionResult GetDynamicPropertyFromAccount([FromODataUri] int key, [FromODataUri] string dynamicProperty)
        {
            return Ok(string.Format("{0}_{1}_{2}", dynamicProperty, "GetDynamicPropertyFromAccount", key));
        }

        [HttpGet]
        [ODataRoute("DynamicCustomers({id})/Order/{pName:dynamicproperty}")]
        public IHttpActionResult GetDynamicPropertyFromOrder([FromODataUri] int id, [FromODataUri] string pName)
        {
            return Ok(string.Format("{0}_{1}_{2}", pName, "GetDynamicPropertyFromOrder", id));
        }
    }

    public class DynamicSingleCustomerController : ODataController
    {
        public IHttpActionResult GetDynamicProperty(string dynamicProperty)
        {
            return Ok(string.Format("{0}_{1}", dynamicProperty, "GetDynamicProperty"));
        }

        public IHttpActionResult GetDynamicPropertyFromAccount([FromODataUri] string dynamicProperty)
        {
            return Ok(string.Format("{0}_{1}", dynamicProperty, "GetDynamicPropertyFromAccount"));
        }

        [HttpGet]
        [ODataRoute("DynamicSingleCustomer/Order/{pName:dynamicproperty}")]
        public IHttpActionResult GetDynamicPropertyFromOrder(string pName)
        {
            return Ok(string.Format("{0}_{1}", pName, "GetDynamicPropertyFromOrder"));
        }
    }
}
