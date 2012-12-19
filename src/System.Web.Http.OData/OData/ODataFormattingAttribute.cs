// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Formatter;
using Microsoft.Data.Edm;
using Microsoft.Data.Edm.Library;

namespace System.Web.Http.OData
{
    /// <summary>
    /// An attribute to be placed on controllers that registers the OData formatters.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ODataFormattingAttribute : Attribute, IControllerConfiguration
    {
        /// <summary>
        /// Callback invoked to set per-controller overrides for this controllerDescriptor.
        /// </summary>
        /// <param name="controllerSettings">The controller settings to initialize.</param>
        /// <param name="controllerDescriptor">The controller descriptor. Note that the <see cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> can be associated with the derived controller type given that <see cref="T:System.Web.Http.Controllers.IControllerConfiguration" /> is inherited.</param>
        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            if (controllerSettings == null)
            {
                throw Error.ArgumentNull("controllerSettings");
            }

            if (controllerDescriptor == null)
            {
                throw Error.ArgumentNull("controllerDescriptor");
            }

            // If any OData formatters are registered globally, do nothing and use those instead
            if (!controllerSettings.Formatters.Where(f => f != null && f.IsODataFormatter()).Any())
            {
                IEdmModel model = controllerDescriptor.Configuration.GetEdmModel();
                if (model != null)
                {
                    controllerSettings.Formatters.InsertRange(0, ODataMediaTypeFormatters.Create(model));
                }
            }
        }
    }
}
