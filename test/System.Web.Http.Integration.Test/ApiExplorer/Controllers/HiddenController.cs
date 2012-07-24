// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Description;
namespace System.Web.Http.ApiExplorer
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HiddenController : ApiController
    {
        public string Get(int id)
        {
            return "visible action";
        }

        [HttpPost]
        public void AddData()
        {
        }

        public int Get()
        {
            return 0;
        }

        [NonAction]
        public string GetHiddenAction()
        {
            return "Hidden action";
        }
    }
}
