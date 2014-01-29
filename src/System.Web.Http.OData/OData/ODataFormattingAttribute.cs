// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using System.Web.Http.OData.Formatter;
using System.Web.Http.Services;

namespace System.Web.Http.OData
{
    /// <summary>
    /// An attribute to be placed on controllers that enables the OData formatters.
    /// </summary>
    /// <remarks>
    /// This attribute does the following actions:
    /// <list type="number">
    /// <item>
    /// <description>
    /// It inserts the ODataMediaTypeFormatters into the <see cref="HttpControllerSettings.Formatters"/> collection.
    /// </description>
    /// </item>
    /// <item><description>It attaches the request to the OData formatter instance.</description></item>
    /// </list>
    /// </remarks>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want to be able to subclass this type.")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ODataFormattingAttribute : Attribute, IControllerConfiguration
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

            // If any OData formatters are registered globally, do nothing and use those instead
            MediaTypeFormatterCollection controllerFormatters = controllerSettings.Formatters;
            if (!controllerFormatters.Where(f => f != null && Decorator.GetInner(f) is ODataMediaTypeFormatter).Any())
            {
                // Remove Xml and Json formatters to avoid media type conflicts.
                RemoveFormatters(controllerFormatters,
                    controllerFormatters.Where(f => f is XmlMediaTypeFormatter || f is JsonMediaTypeFormatter));

                // Then add our formatters.
                controllerFormatters.InsertRange(0, CreateODataFormatters());
            }

            ServicesContainer services = controllerSettings.Services;
            Contract.Assert(services != null);

            // Replace the action value binder with one that attaches the request to the formatter.
            IActionValueBinder originalActionValueBinder = services.GetActionValueBinder();
            IActionValueBinder actionValueBinder = new PerRequestActionValueBinder(originalActionValueBinder);
            controllerSettings.Services.Replace(typeof(IActionValueBinder), actionValueBinder);

            // Replace the content negotiator with one that uses the per-request formatters.
            // This negotiator is required to allow CanReadType to have access to the model.
            IContentNegotiator originalContentNegotiator = services.GetContentNegotiator();
            IContentNegotiator contentNegotiator = new PerRequestContentNegotiator(originalContentNegotiator);
            controllerSettings.Services.Replace(typeof(IContentNegotiator), contentNegotiator);
        }

        /// <summary>
        /// Creates the OData formatters.
        /// </summary>
        /// <returns>A collection of OData formatters.</returns>
        public virtual IList<ODataMediaTypeFormatter> CreateODataFormatters()
        {
            return ODataMediaTypeFormatters.Create();
        }

        private static void RemoveFormatters(MediaTypeFormatterCollection formatterCollection,
            IEnumerable<MediaTypeFormatter> formattersToRemove)
        {
            Contract.Assert(formatterCollection != null);
            Contract.Assert(formattersToRemove != null);

            // Instantiate a separate array to isolate enumeration from deletions. This code would otherwise throw
            // after the first removal.
            foreach (MediaTypeFormatter formatter in formattersToRemove.ToArray())
            {
                formatterCollection.Remove(formatter);
            }
        }
    }
}
