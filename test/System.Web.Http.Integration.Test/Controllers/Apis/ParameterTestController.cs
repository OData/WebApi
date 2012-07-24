// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http
{
    public class ParameterTestController : ApiController
    {
        public void Delete(int id)
        {
        }

        public string Get(int id = -1)
        {
            return String.Format("Get({0})", id);
        }

        public string POST(string id = null)
        {
            return String.Format("POST({0})", id ?? "null");
        }

        public string Put(int id, string value)
        {
            return String.Format("Put({0}, {1})", id, value);
        }
    }
}
