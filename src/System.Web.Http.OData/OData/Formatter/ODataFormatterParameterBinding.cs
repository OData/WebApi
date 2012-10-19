// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

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
        private MediaTypeFormatter _formatter;

        public ODataFormatterParameterBinding(HttpParameterDescriptor descriptor, MediaTypeFormatter formatter)
            : base(descriptor)
        {
            if (formatter == null)
            {
                throw Error.ArgumentNull("formatter");
            }
            _formatter = formatter;
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var formatter = _formatter.GetPerRequestFormatterInstance(Descriptor.ParameterType, actionContext.Request, actionContext.Request.Content.Headers.ContentType);
            return Descriptor.BindWithFormatter(new[] { formatter }).ExecuteBindingAsync(metadataProvider, actionContext, cancellationToken);
        }
    }
}
