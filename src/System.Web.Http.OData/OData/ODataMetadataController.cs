// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;

namespace System.Web.Http.OData
{
    [ODataMetadataControllerConfiguration]
    public class ODataMetadataController : ApiController
    {
        private static readonly Version _defaultEdmxVersion = new Version(1, 0);
        private static readonly Version _defaultDataServiceVersion = new Version(1, 0);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property not appropriate")]
        public IEdmModel GetMetadata()
        {
            return GetModel();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property not appropriate")]
        public ODataWorkspace GetServiceDocument()
        {
            ODataWorkspace workspace = new ODataWorkspace();
            IEdmModel model = GetModel();
            Contract.Assert(model != null);
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmEntitySet> entitysets = container.EntitySets();

            IEnumerable<ODataResourceCollectionInfo> collections = entitysets.Select(e => GetODataResourceCollectionInfo(model.GetEntitySetUrl(e).ToString(), e.Name));
            workspace.Collections = collections;

            return workspace;
        }

        private static ODataResourceCollectionInfo GetODataResourceCollectionInfo(string url, string name)
        {
            ODataResourceCollectionInfo info = new ODataResourceCollectionInfo
            {
                Url = new Uri(url, UriKind.Relative)
            };

            info.SetAnnotation<AtomResourceCollectionMetadata>(new AtomResourceCollectionMetadata { Title = name });

            return info;
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property not appropriate")]
        protected virtual IEdmModel GetModel()
        {
            IEdmModel model = Request.GetConfiguration().Formatters.ODataFormatter().Model;
            if (model == null)
            {
                throw Error.NotSupported(SRResources.ODataFormatterMissing, typeof(ODataMediaTypeFormatter).Name);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            model.SetDataServiceVersion(_defaultDataServiceVersion);
            return model;
        }
    }
}
