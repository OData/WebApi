// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


namespace System.Web.Http.ApiExplorer
{
    [ApiDocumentation("Documentation controller")]
    public class DocumentationController : ApiController
    {
        [ApiDocumentation("Get action")]
        [ApiResponseDocumentation("Get response")]
        public string Get()
        {
            return string.Empty;
        }

        [ApiDocumentation("Post action")]
        [ApiParameterDocumentation("value", "value parameter")]
        public void Post(string value)
        {
        }

        [ApiDocumentation("Put action")]
        [ApiParameterDocumentation("id", "id parameter")]
        [ApiParameterDocumentation("value", "value parameter")]
        public void Put(int id, string value)
        {
        }

        [ApiDocumentation("Delete action")]
        [ApiParameterDocumentation("id", "id parameter")]
        public void Delete(int id)
        {
        }
    }
}
