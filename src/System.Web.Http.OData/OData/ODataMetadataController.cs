// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;

namespace System.Web.Http.OData
{
    public class ODataMetadataController : ODataController
    {
        private static readonly Version _defaultEdmxVersion = new Version(1, 0);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Property not appropriate")]
        public IEdmModel GetMetadata()
        {
            return GetModel();
        }

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
