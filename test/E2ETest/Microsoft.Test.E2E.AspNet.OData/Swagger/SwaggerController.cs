//-----------------------------------------------------------------------------
// <copyright file="SwaggerController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;
using Microsoft.Test.E2E.AspNet.OData.Common.Controllers;
using Newtonsoft.Json.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.Swagger
{
    public class SwaggerController : TestNonODataController
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
