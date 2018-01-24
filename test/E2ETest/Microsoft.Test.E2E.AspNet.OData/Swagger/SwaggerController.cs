// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Newtonsoft.Json.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.Swagger
{
    public class SwaggerController : ApiController
    {
        private static readonly Version _defaultEdmxVersion = new Version(4, 0);

        [EnableQuery]
        public JObject GetSwagger()
        {
            IEdmModel model = Request.GetModel();
            model.SetEdmxVersion(_defaultEdmxVersion);
            ODataSwaggerConverter converter = new ODataSwaggerConverter(model);
            return converter.GetSwaggerModel();
        }
    }
}
