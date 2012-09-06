// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.OData.Formatter;

namespace System.Web.Http.OData
{
    internal sealed class ODataMetadataControllerConfigurationAttribute : Attribute, IControllerConfiguration
    {
        // This class requires and supports only OData formatter. So clear everything else.
        // This also helps the case where a request without accept header hitting the metadata endpoint. we want the metadata to be serialized in odata format. 
        // if the accept header contains anything other than odata format, we should fail.
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerSettings == null)
            {
                throw Error.ArgumentNull("controllerSettings");
            }

            ODataMediaTypeFormatter odataFormatter = controllerDescriptor.Configuration.GetODataFormatter();
            controllerSettings.Formatters.Clear();

            if (odataFormatter != null)
            {
                controllerSettings.Formatters.Add(odataFormatter);
            }
        }
    }
}
