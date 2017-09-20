// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a controller for generating OData servicedoc and metadata document ($metadata).
    /// </summary>
    public partial class MetadataController
    {
        private static readonly Version _defaultEdmxVersion = new Version(4, 0);

        private IEdmModel GetModel()
        {
            IEdmModel model = Request.GetModel();
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            return model;
        }
    }
}
