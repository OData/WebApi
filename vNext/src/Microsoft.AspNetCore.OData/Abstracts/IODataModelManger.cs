// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Abstracts
{
    /// <summary>
    /// Provides programmatic configuration for the OData service.
    /// </summary>
    public interface IODataModelManger
    {
        IEdmModel GetModel(string key);

        void AddModel(string key, IEdmModel model);
    }
}
