//-----------------------------------------------------------------------------
// <copyright file="NorthwindController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Web.Http;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers.TypeLibrary;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
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
