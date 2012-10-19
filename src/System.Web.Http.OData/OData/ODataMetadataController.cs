// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Formatter;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;

namespace System.Web.Http.OData
{
    public class ODataMetadataController : ApiController
    {
        private static readonly Version _defaultEdmxVersion = new Version(1, 0);
        private static readonly Version _defaultDataServiceVersion = new Version(1, 0);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property not appropriate")]
        public HttpResponseMessage GetMetadata()
        {
            IEdmModel model;
            MediaTypeFormatter odataFormatter = GetFormatter(typeof(IEdmModel), out model);

            // This controller requires and supports only the OData formatter.
            return Request.CreateResponse(HttpStatusCode.OK, model, odataFormatter);
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Property not appropriate")]
        public HttpResponseMessage GetServiceDocument()
        {
            IEdmModel model;
            MediaTypeFormatter odataFormatter = GetFormatter(typeof(ODataWorkspace), out model);

            ODataWorkspace workspace = new ODataWorkspace();
            IEdmEntityContainer container = model.EntityContainers().Single();
            IEnumerable<IEdmEntitySet> entitysets = container.EntitySets();

            IEnumerable<ODataResourceCollectionInfo> collections = entitysets.Select(e => GetODataResourceCollectionInfo(model.GetEntitySetUrl(e).ToString(), e.Name));
            workspace.Collections = collections;

            // This controller requires and supports only the OData formatter.
            return Request.CreateResponse(HttpStatusCode.OK, workspace, odataFormatter);
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

        private MediaTypeFormatter GetFormatter(Type responseType, out IEdmModel model)
        {
            HttpConfiguration configuration = Request.GetConfiguration();
            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            MediaTypeFormatter odataFormatter = configuration.GetODataFormatter(out model);
            if (odataFormatter == null)
            {
                throw Error.InvalidOperation(SRResources.NoODataFormatterForMetadata);
            }

            model.SetEdmxVersion(_defaultEdmxVersion);
            model.SetDataServiceVersion(_defaultDataServiceVersion);
            odataFormatter = odataFormatter.GetPerRequestFormatterInstance(responseType, Request, mediaType: null);
            return odataFormatter;
        }
    }
}
