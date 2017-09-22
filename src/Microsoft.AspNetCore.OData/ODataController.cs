// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Defines a base class for OData controllers that support writing and reading data using the OData formats.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract partial class ODataController : ControllerBase
    {
        private static readonly Version _defaultEdmxVersion = new Version(4, 0);

        /// <summary>
        /// Get the Edm model associated with the controller.
        /// </summary>
        /// <returns></returns>
        protected IEdmModel GetModel()
        {
            IEdmModel model = GetModel();
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            return model;
        }
    }
}
