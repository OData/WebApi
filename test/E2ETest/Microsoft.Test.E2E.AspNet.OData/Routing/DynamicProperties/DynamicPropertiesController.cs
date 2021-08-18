//-----------------------------------------------------------------------------
// <copyright file="DynamicPropertiesController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Routing;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;

namespace Microsoft.Test.E2E.AspNet.OData.Routing.DynamicProperties
{
    public class DynamicCustomersController : TestODataController
    {
        public ITestActionResult GetId(int key)
        {
            return Ok(string.Format("{0}_{1}", "Id", key));
        }

        public ITestActionResult GetDynamicProperty(int key, string dynamicProperty)
        {
            return Ok(string.Format("{0}_{1}_{2}", dynamicProperty, "GetDynamicProperty", key));
        }

        public ITestActionResult GetDynamicPropertyFromAccount([FromODataUri] int key, [FromODataUri] string dynamicProperty)
        {
            return Ok(string.Format("{0}_{1}_{2}", dynamicProperty, "GetDynamicPropertyFromAccount", key));
        }

        [HttpGet]
        [ODataRoute("DynamicCustomers({id})/Order/{pName:dynamicproperty}")]
        public ITestActionResult GetDynamicPropertyFromOrder([FromODataUri] int id, [FromODataUri] string pName)
        {
            return Ok(string.Format("{0}_{1}_{2}", pName, "GetDynamicPropertyFromOrder", id));
        }
    }

    public class DynamicSingleCustomerController : TestODataController
    {
        public ITestActionResult GetDynamicProperty(string dynamicProperty)
        {
            return Ok(string.Format("{0}_{1}", dynamicProperty, "GetDynamicProperty"));
        }

        public ITestActionResult GetDynamicPropertyFromAccount([FromODataUri] string dynamicProperty)
        {
            return Ok(string.Format("{0}_{1}", dynamicProperty, "GetDynamicPropertyFromAccount"));
        }

        [HttpGet]
        [ODataRoute("DynamicSingleCustomer/Order/{pName:dynamicproperty}")]
        public ITestActionResult GetDynamicPropertyFromOrder(string pName)
        {
            return Ok(string.Format("{0}_{1}", pName, "GetDynamicPropertyFromOrder"));
        }
    }
}
