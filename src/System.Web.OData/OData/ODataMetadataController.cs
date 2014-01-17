// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.Atom;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Csdl;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Represents an <see cref="ApiController"/> for generating OData servicedoc and metadata document ($metadata).
    /// </summary>
    public class ODataMetadataController : ODataController
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
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmEntitySet> entitysets = container.EntitySets();

            IEnumerable<ODataEntitySetInfo> entitySets = entitysets.Select(
                e => GetODataEntitySetInfo(model.GetEntitySetUrl(e).ToString(), e.Name));
            serviceDocument.EntitySets = entitySets;

            // TODO: 1601 Add FunctionImports and Singletons to service document

            return serviceDocument;
        }

        private static ODataEntitySetInfo GetODataEntitySetInfo(string url, string name)
        {
            ODataEntitySetInfo info = new ODataEntitySetInfo
            {
                Name = name, // Required for JSON light support
                Url = new Uri(url, UriKind.Relative)
            };

            info.SetAnnotation<AtomResourceCollectionMetadata>(new AtomResourceCollectionMetadata { Title = name });

            return info;
        }

        private IEdmModel GetModel()
        {
            IEdmModel model = Request.GetEdmModel();
            if (model == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustHaveModel);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            return model;
        }
    }
}
