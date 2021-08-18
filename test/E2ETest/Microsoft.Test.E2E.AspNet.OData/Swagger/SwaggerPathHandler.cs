//-----------------------------------------------------------------------------
// <copyright file="SwaggerPathHandler.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
