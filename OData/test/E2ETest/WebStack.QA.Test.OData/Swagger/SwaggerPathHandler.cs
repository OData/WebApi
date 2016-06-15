using System;
using System.Web.OData.Routing;
using Microsoft.OData.Edm;

namespace WebStack.QA.Test.OData.Swagger
{
    public class SwaggerPathHandler : DefaultODataPathHandler
    {
        public override ODataPath Parse(IEdmModel model, string serviceRoot, string odataPath, IServiceProvider requestContainer)
        {
            ODataPath path;
            try
            {
                path = base.Parse(model, serviceRoot, odataPath, requestContainer);
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
