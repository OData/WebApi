// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Routing;

namespace Microsoft.Test.E2E.AspNet.OData.Swagger
{
    public class SwaggerPathHandler : DefaultODataPathHandler
    {
        public override ODataPath Parse(string serviceRoot, string odataPath, IServiceProvider requestContainer)
        {
            ODataPath path;
            try
            {
                path = base.Parse(serviceRoot, odataPath, requestContainer);
                return path;
            }
            catch (Exception)
            {
                if (IsSwaggerMetadataUri(odataPath))
                {
                    return new ODataPath(new SwaggerPathSegment());
                }

                throw;
            }
        }

        private static bool IsSwaggerMetadataUri(string odataPath)
        {
            string unescapeODataPath = Uri.UnescapeDataString(odataPath);
            switch (unescapeODataPath)
            {
                case "$swagger":
                case "swagger.json":
                    return true;
            }

            return false;
        }
    }
}
