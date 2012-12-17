// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.Description;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Properties;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Csdl;
using Microsoft.Data.OData;
using Microsoft.Data.OData.Atom;

namespace System.Web.Http.OData
{
    [PerControllerConfiguration]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ODataMetadataController : ApiController
    {
        private static readonly Version _defaultEdmxVersion = new Version(1, 0);
        private static readonly Version _defaultDataServiceVersion = new Version(1, 0);

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
            HttpConfiguration configuration = Request.GetConfiguration();

            if (configuration == null)
            {
                throw Error.InvalidOperation(SRResources.RequestMustContainConfiguration);
            }

            MediaTypeFormatter firstODataFormatter = configuration.Formatters.FirstOrDefault(
                f => f != null && f.IsODataFormatter());

            if (firstODataFormatter == null)
            {
                throw Error.InvalidOperation(SRResources.NoODataFormatterForMetadata);
            }

            IEdmModel model = firstODataFormatter.GetODataModel();
            Contract.Assert(model != null);
            model.SetEdmxVersion(_defaultEdmxVersion);
            model.SetDataServiceVersion(_defaultDataServiceVersion);
            return model;
        }

        private sealed class PerControllerConfigurationAttribute : Attribute, IControllerConfiguration
        {
            public void Initialize(HttpControllerSettings controllerSettings,
                HttpControllerDescriptor controllerDescriptor)
            {
                if (controllerSettings == null)
                {
                    throw Error.ArgumentNull("controllerSettings");
                }

                if (controllerDescriptor == null)
                {
                    throw Error.ArgumentNull("controllerDescriptor");
                }

                MediaTypeFormatterCollection formatters = controllerSettings.Formatters;
                Contract.Assert(formatters != null);

                // Only remove the non-OData formatters if at least one OData formatter exists. Otherwise, nothing will
                // be left to serialize error messages (like the error message indicating that no OData formatter
                // exists).
                bool hasODataFormatter = formatters.Any(f => f != null && f.IsODataFormatter());

                if (hasODataFormatter)
                {
                    IEnumerable<MediaTypeFormatter> nonODataFormatters = formatters.Where(
                        f => f == null || !f.IsODataFormatter());
                    formatters.RemoveRange(nonODataFormatters);
                }
            }
        }
    }
}
