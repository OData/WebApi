// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.ApiExplorer
{
    public class AmbiguousActionController : ApiController
    {
        // The actions GetItem and Get and potentially be ambiguous under
        // route "api/{controller}" because they're both GET api/AmbiguousAction

        public void GetItem()
        {
        }

        public void Get()
        {
        }

        public bool Post(int id)
        {
            return true;
        }
    }
}