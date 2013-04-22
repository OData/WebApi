// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.Cors
{
    public class DefaultController : ApiController
    {
        public string Get()
        {
            return "value";
        }

        [EnableCors("http://restrictedExample.com", "*", "*")]
        public string Post()
        {
            return "value created";
        }
    }
}