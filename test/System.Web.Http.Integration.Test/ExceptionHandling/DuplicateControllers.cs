// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.Http
{
    public class DuplicateController : ApiController
    {
        public string GetAction()
        {
            return "dup";
        }
    }
}

namespace System.Web.Http2
{
    public class DuplicateController : ApiController
    {
        public string GetAction()
        {
            return "dup2";
        }
    }
}
