// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Metadata;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// This describes *how* the binding will happen. Does not actually bind. 
    /// This is static for a given action descriptor and can be reused across requests. 
    /// This may be a nice thing to log. Or set a breakpoint after we create and preview what's about to happen. 
    /// In theory, this could be precompiled for each Action descriptor.  
    /// </summary>
    public class HttpActionBinding
    {
        private HttpActionDescriptor _actionDescriptor;
        private HttpParameterBinding[] _parameterBindings;

        private ModelMetadataProvider _metadataProvider;

        public HttpActionBinding()
        {
        }

        public HttpActionBinding(HttpActionDescriptor actionDescriptor, HttpParameterBinding[] bindings)
        {
            ActionDescriptor = actionDescriptor;
            ParameterBindings = bindings;
        }

        /// <summary>
        /// Back pointer to the action this binding is for. 
        /// This can also provide the Type[], string[] names for the parameters.
        /// </summary>
        public HttpActionDescriptor ActionDescriptor
        {
            get
            {
                return _actionDescriptor;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _actionDescriptor = value;
            }
        }

        /// <summary>
        /// Specifies synchronous bindings for each parameter.This is a parallel array to the ActionDescriptor's parameter array. 
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Want an array")]
        public HttpParameterBinding[] ParameterBindings
        {
            get
            {
                return _parameterBindings;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                _parameterBindings = value;
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Caller is responsible for disposing of response instance.")]
        public virtual Task ExecuteBindingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (_parameterBindings.Length == 0)
            {
                return TaskHelpers.Completed();
            }

            // First, make sure the actionBinding is valid before trying to execute it. This keeps us in a known state in case of errors.
            foreach (HttpParameterBinding parameterBinder in ParameterBindings)
            {
                if (!parameterBinder.IsValid)
                {
                    // Error code here is 500 because the WebService developer's action signature is bad. 
                    return TaskHelpers.FromError(new HttpResponseException(actionContext.Request.CreateResponse(
                        HttpStatusCode.InternalServerError, parameterBinder.ErrorMessage)));
                }
            }

            if (_metadataProvider == null)
            {
                HttpConfiguration config = actionContext.ControllerContext.Configuration;
                _metadataProvider = config.Services.GetModelMetadataProvider();
            }

            // Execute all the binders.
            IEnumerable<Task> tasks = from parameterBinder in ParameterBindings select parameterBinder.ExecuteBindingAsync(_metadataProvider, actionContext, cancellationToken);
            return TaskHelpers.Iterate(tasks, cancellationToken);
        }
    }
}
