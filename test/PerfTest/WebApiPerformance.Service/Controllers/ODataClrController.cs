//-----------------------------------------------------------------------------
// <copyright file="ODataClrController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Microsoft.AspNet.OData;

namespace WebApiPerformance.Service
{
    public class ODataClrController : ODataController
    {
        [HttpGet]
        public ClassA Get(string key)
        {
            key = key.Trim('\'');
            var item = TestRepo.GetAs().FirstOrDefault(a => key.Equals(a.Name));

            return item;
        }

        [HttpGet, EnableQuery]
        public IEnumerable<ClassA> Get()
        {
            int n;
            n =
                int.TryParse(
                    Request.GetQueryNameValuePairs().Where(kv => kv.Key == "n").Select(kv => kv.Value).FirstOrDefault(), out n)
                    ? n
                    : 10;
            return TestRepo.GetAs(n);
        }


        [HttpPost]
        public IHttpActionResult Post(ClassA a)
        {
            return Ok(a);
        }
    }
}
