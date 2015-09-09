// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace System.Web.OData
{
    /// <summary>
    /// Represents an <see cref="ApiController"/> for generating OData swagger document ($swagger).
    /// </summary>
    public class SwaggerController : ODataController
    {
        private static readonly Version _defaultEdmxVersion = new Version(4, 0);

        /// <summary>
        /// Generates the OData $swagger document.
        /// </summary>
        /// <returns>The <see cref="SwaggerModel"/> representing $swagger.</returns>
        public SwaggerModel GetSwagger()
        {
            IEdmModel model = Request.ODataProperties().Model;
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            return new SwaggerModel(model);
        }
    }
}
