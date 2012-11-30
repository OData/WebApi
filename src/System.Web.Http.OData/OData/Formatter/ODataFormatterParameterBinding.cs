// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// A special HttpParameterBinding that uses a Per Request formatter instance with access to the Request.
    /// <remarks>
    /// This class is needed by some of the ODataDeserializers, since they actually need access to more than just the Request body,
    /// they also need to interrogate the RequestUri etc.
    /// </remarks>
    /// </summary>
    public class ODataFormatterParameterBinding : HttpParameterBinding
    {
        private IEnumerable<MediaTypeFormatter> _formatters;

        public ODataFormatterParameterBinding(HttpParameterDescriptor descriptor,
            IEnumerable<MediaTypeFormatter> formatters)
            : base(descriptor)
        {
            if (formatters == null)
            {
                throw Error.ArgumentNull("formatters");
            }

            _formatters = formatters;
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            List<MediaTypeFormatter> perRequestFormatters = new List<MediaTypeFormatter>();

            foreach (MediaTypeFormatter formatter in _formatters)
            {
                MediaTypeFormatter perRequestFormatter = formatter.GetPerRequestFormatterInstance(Descriptor.ParameterType, actionContext.Request, actionContext.Request.Content.Headers.ContentType);
                perRequestFormatters.Add(perRequestFormatter);
            }

            return Descriptor.BindWithFormatter(perRequestFormatters).ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken);
        }
    }
}
