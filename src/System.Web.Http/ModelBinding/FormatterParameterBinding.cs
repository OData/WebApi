// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.Http.Validation;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Parameter binding that will read from the body and invoke the formatters. 
    /// </summary>
    public class FormatterParameterBinding : HttpParameterBinding
    {
        private IEnumerable<MediaTypeFormatter> _formatters;

        public FormatterParameterBinding(HttpParameterDescriptor descriptor, IEnumerable<MediaTypeFormatter> formatters, IBodyModelValidator bodyModelValidator)
            : base(descriptor)
        {
            Formatters = formatters;
            BodyModelValidator = bodyModelValidator;
        }

        public override bool WillReadBody
        {
            get { return true; }
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

        protected virtual Task<object> ReadContentAsync(HttpRequestMessage request, Type type, IEnumerable<MediaTypeFormatter> formatters, IFormatterLogger formatterLogger)
        {
            return request.Content.ReadAsAsync(type, formatters, formatterLogger);
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
                    string name = paramFromBody.ParameterName;
                    actionContext.ActionArguments.Add(name, model);

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
