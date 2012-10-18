// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
#if Debug
using System.IO;
using System.Web.Hosting;
using System.Web.Http;

namespace Microsoft.AspNet.Mvc.Facebook.Controllers
{
    public class FacebookRealTimeLogController : ApiController
    {
        // GET api/facebookrealtimelog
        public string Get()
        {
            return File.ReadAllText(HostingEnvironment.ApplicationPhysicalPath + "\\Log.txt");
            //return new string[] { "value1", "value2" };
        }

        // GET api/facebookrealtimelog/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/facebookrealtimelog
        public void Post([FromBody]string value)
        {
        }

        // PUT api/facebookrealtimelog/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/facebookrealtimelog/5
        public void Delete(int id)
        {
        }
    }
}
#endif