// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace Microsoft.Web.Http.Data.Helpers
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class MetadataExtensions
    {
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification = "Following established design pattern for HTML helpers.")]
        public static IHtmlString Metadata<TDataController>(this HtmlHelper htmlHelper) where TDataController : DataController
        {
            HttpControllerDescriptor controllerDescriptor = new HttpControllerDescriptor
            {
                Configuration = GlobalConfiguration.Configuration, // This helper can't be run until after global app init.
                ControllerType = typeof(TDataController)
            };

            DataControllerDescription description = DataControllerDescription.GetDescription(controllerDescriptor);
            IEnumerable<DataControllerMetadataGenerator.TypeMetadata> metadata =
                DataControllerMetadataGenerator.GetMetadata(description);

            JToken metadataValue = new JObject(metadata.Select(
                m => new KeyValuePair<string, JToken>(m.EncodedTypeName, m.ToJToken())));

            return htmlHelper.Raw(metadataValue);
        }
    }
}
