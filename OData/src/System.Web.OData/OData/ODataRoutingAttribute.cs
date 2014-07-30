// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ValueProviders;
using System.Web.OData.Routing;

namespace System.Web.OData
{
    /// <summary>
    /// Defines a controller-level attribute that can be used to enable OData action selection based on routing conventions.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ODataRoutingAttribute : Attribute, IControllerConfiguration
    {
        /// <summary>
        /// Callback invoked to set per-controller overrides for this controllerDescriptor.
        /// </summary>
        /// <param name="controllerSettings">The controller settings to initialize.</param>
        /// <param name="controllerDescriptor">The controller descriptor. Note that the <see
        /// cref="T:System.Web.Http.Controllers.HttpControllerDescriptor" /> can be associated with the derived
        /// controller type given that <see cref="T:System.Web.Http.Controllers.IControllerConfiguration" /> is
        /// inherited.</param>
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

            ServicesContainer services = controllerSettings.Services;
            Contract.Assert(services != null);

            // Replace the action selector with one that is based on the OData routing conventions
            IHttpActionSelector originalActionSelector = services.GetActionSelector();
            IHttpActionSelector actionSelector = new ODataActionSelector(originalActionSelector);
            services.Replace(typeof(IHttpActionSelector), actionSelector);

            services.Insert(typeof(ValueProviderFactory), 0, new ODataValueProviderFactory());
        }
    }
}