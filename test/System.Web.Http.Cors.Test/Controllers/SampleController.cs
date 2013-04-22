// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Cors
{
    [EnableCors("*", "*", "*")]
    public class SampleController : ApiController
    {
        public string Get()
        {
            return "value";
        }

        [DisableCors]
        public string Get(int id)
        {
            return "value" + id;
        }

        [DisableCors]
        public string Post()
        {
            return "value";
        }

        public void Delete()
        {
        }

        [EnableCors("http://example.com", "*", "*")]
        public void Head()
        {
        }

        [EnableCors("http://example.com, http://localhost", "*", "*")]
        public void Put()
        {
        }
    }
}