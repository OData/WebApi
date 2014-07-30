// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;

namespace System.Web.Http.OData
{
    /// <summary>
    /// Represents an <see cref="ApiController"/> for generating OData servicedoc and metadata document ($metadata).
    /// </summary>
    public class ODataMetadataController : ODataController
    {
        private static readonly Version _defaultEdmxVersion = new Version(1, 0);

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
        public ODataWorkspace GetServiceDocument()
        {
            IEdmModel model = GetModel();
            ODataWorkspace workspace = new ODataWorkspace();
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmEntitySet> entitysets = container.EntitySets();

            IEnumerable<ODataResourceCollectionInfo> collections = entitysets.Select(
                e => GetODataResourceCollectionInfo(model.GetEntitySetUrl(e).ToString(), e.Name));
            workspace.Collections = collections;

            return workspace;
        }

        private static ODataResourceCollectionInfo GetODataResourceCollectionInfo(string url, string name)
        {
            ODataResourceCollectionInfo info = new ODataResourceCollectionInfo
            {
                Name = name, // Required for JSON light support
                Url = new Uri(url, UriKind.Relative)
            };

            info.SetAnnotation<AtomResourceCollectionMetadata>(new AtomResourceCollectionMetadata { Title = name });

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
