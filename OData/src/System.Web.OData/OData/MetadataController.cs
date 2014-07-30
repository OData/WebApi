// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace System.Web.OData
{
    /// <summary>
    /// Represents an <see cref="ApiController"/> for generating OData servicedoc and metadata document ($metadata).
    /// </summary>
    public class MetadataController : ODataController
    {
        private static readonly Version _defaultEdmxVersion = new Version(4, 0);

        /// <summary>
        /// Generates the OData $metadata document.
        /// </summary>
        /// <returns>The <see cref="IEdmModel"/> representing $metadata.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Property not appropriate")]
        public IEdmModel GetMetadata()
        {
            return GetModel();
        }

        /// <summary>
        /// Generates the OData service document.
        /// </summary>
        /// <returns>The service document for the service.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Property not appropriate")]
        public ODataServiceDocument GetServiceDocument()
        {
            IEdmModel model = GetModel();
            ODataServiceDocument serviceDocument = new ODataServiceDocument();
            IEdmEntityContainer container = model.EntityContainer;

            // Add EntitySets into service document
            serviceDocument.EntitySets = container.EntitySets().Select(
                e => GetODataEntitySetInfo(model.GetNavigationSourceUrl(e).ToString(), e.Name));

            // Add Singletons into the service document
            IEnumerable<IEdmSingleton> singletons = container.Elements.OfType<IEdmSingleton>();
            serviceDocument.Singletons = singletons.Select(
                e => GetODataSingletonInfo(model.GetNavigationSourceUrl(e).ToString(), e.Name));

            // Add FunctionImports into service document
            // ODL spec says:
            // The edm:FunctionImport for a parameterless function MAY include the IncludeInServiceDocument attribute
            // whose Boolean value indicates whether the function import is advertised in the service document.
            // If no value is specified for this attribute, its value defaults to false.

            // Find all parameterless functions with "IncludeInServiceDocument = true"
            IEnumerable<IEdmFunctionImport> functionImports = container.Elements.OfType<IEdmFunctionImport>()
                .Where(f => !f.Function.Parameters.Any() && f.IncludeInServiceDocument);

            serviceDocument.FunctionImports = functionImports.Distinct(new FunctionImportComparer())
                .Select(f => GetODataFunctionImportInfo(f.Name));

            return serviceDocument;
        }

        private static ODataEntitySetInfo GetODataEntitySetInfo(string url, string name)
        {
            ODataEntitySetInfo info = new ODataEntitySetInfo
            {
                Name = name, // Required for JSON support
                Url = new Uri(url, UriKind.Relative)
            };

            return info;
        }

        private static ODataSingletonInfo GetODataSingletonInfo(string url, string name)
        {
            ODataSingletonInfo info = new ODataSingletonInfo
            {
                Name = name,
                Url = new Uri(url, UriKind.Relative)
            };

            return info;
        }

        private static ODataFunctionImportInfo GetODataFunctionImportInfo(string name)
        {
            ODataFunctionImportInfo info = new ODataFunctionImportInfo
            {
                Name = name,
                Url = new Uri(name, UriKind.Relative) // Relative to the OData root
            };

            return info;
        }

        private IEdmModel GetModel()
        {
            IEdmModel model = Request.ODataProperties().Model;
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            return model;
        }
    }
}
