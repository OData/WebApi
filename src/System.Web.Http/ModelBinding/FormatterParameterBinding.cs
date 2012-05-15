// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Properties;
using System.Web.Http.Validation;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Parameter binding that will read from the body and invoke the formatters. 
    /// </summary>
    public class FormatterParameterBinding : HttpParameterBinding
    {
        private IEnumerable<MediaTypeFormatter> _formatters;
        private string _errorMessage;

        public FormatterParameterBinding(HttpParameterDescriptor descriptor, IEnumerable<MediaTypeFormatter> formatters, IBodyModelValidator bodyModelValidator)
            : base(descriptor)
        {
            if (descriptor.IsOptional)
            {
                _errorMessage = Error.Format(SRResources.OptionalBodyParameterNotSupported, descriptor.Prefix ?? descriptor.ParameterName, GetType().Name);
            }
            Formatters = formatters;
            BodyModelValidator = bodyModelValidator;
        }

        public override bool WillReadBody
        {
            get { return true; }
        }

        public override string ErrorMessage
        {
            get
            {
                return _errorMessage;
            }
        }

        public IEnumerable<MediaTypeFormatter> Formatters
        {
            get { return _formatters; }
            set
            {
                if (value == null)
                {
                    throw Error.ArgumentNull("formatters");
                }
                _formatters = value;
            }
        }

        public IBodyModelValidator BodyModelValidator
        {
            get;
            set;
        }

        public virtual Task<object> ReadContentAsync(HttpRequestMessage request, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            HttpContent content = request.Content;
            if (content == null)
            {
                object defaultValue = MediaTypeFormatter.GetDefaultValueForType(type);
                if (defaultValue == null)
                {
                    return TaskHelpers.NullResult();
                }
                else
                {
                    return TaskHelpers.FromResult(defaultValue);
                }
            }
            return content.ReadAsAsync(type, formatters, formatterLogger);
        }

        public override Task ExecuteBindingAsync(ModelMetadataProvider metadataProvider, HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            HttpParameterDescriptor paramFromBody = this.Descriptor;
            Type type = paramFromBody.ParameterType;
            HttpRequestMessage request = actionContext.ControllerContext.Request;
            IFormatterLogger formatterLogger = new ModelStateFormatterLogger(actionContext.ModelState, paramFromBody.ParameterName);
            Task<object> task = ReadContentAsync(request, type, _formatters, formatterLogger);

            return task.Then(
                (model) =>
                {
                    // Put the parameter result into the action context.
                    SetValue(actionContext, model);

                    // validate the object graph. 
                    // null indicates we want no body parameter validation
                    if (BodyModelValidator != null)
                    {
                        BodyModelValidator.Validate(model, type, metadataProvider, actionContext, paramFromBody.ParameterName);
                    }
                });
        }
    }
}
