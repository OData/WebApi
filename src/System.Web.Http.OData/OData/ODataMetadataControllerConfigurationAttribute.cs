// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http.Controllers;
using System.Web.Http.OData.Formatter;

namespace System.Web.Http.OData
{
    internal sealed class ODataMetadataControllerConfigurationAttribute : Attribute, IControllerConfiguration
    {
        // This class requires and supports only OData formatter. So clear everything else.
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerSettings == null)
            {
                throw Error.ArgumentNull("controllerSettings");
            }

            ODataMediaTypeFormatter odataFormatter = controllerSettings.Formatters.ODataFormatter();
            controllerSettings.Formatters.Clear();

            if (odataFormatter != null)
            {
                controllerSettings.Formatters.Add(odataFormatter);
            }
        }
    }
}
