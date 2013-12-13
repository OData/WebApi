// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Specialized;
using System.Net.Http;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Provides an object representation for an unresolved parameter value of function.
    /// </summary>
    public class UnresolvedParameterValue
    {
        /// <summary>
        /// Gets the EDM model containing the function.
        /// </summary>
        public IEdmModel EdmModel { get; private set; }

        /// <summary>
        /// Gets the EDM type of the parameter.
        /// </summary>
        public IEdmTypeReference EdmType { get; private set; }

        /// <summary>
        /// Gets or sets the alias of the parameter.
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnresolvedParameterValue" /> class.
        /// </summary>
        /// <param name="type">The <see cref="IEdmTypeReference"/> of the parameter.</param>
        /// <param name="alias">The alias of the parameter.</param>
        /// <param name="model">The <see cref="IEdmModel"/> containing the function.</param>
        public UnresolvedParameterValue(IEdmTypeReference type, string alias, IEdmModel model)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }
            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }
            if (String.IsNullOrEmpty("alias"))
            {
                throw Error.ArgumentNullOrEmpty("alias");
            }

            EdmType = type;
            Alias = alias;
            EdmModel = model;
        }

        /// <summary>
        /// Resolves the parameter value in the URI query string.
        /// </summary>
        /// <param name="uri">The URI with the query string which contains the parameter value.</param>
        public object Resolve(Uri uri)
        {
            if (uri == null)
            {
                throw Error.ArgumentNull("uri");
            }

            return Resolve(UriExtensions.ParseQueryString(uri));
        }

        /// <summary>
        /// Resolves the parameter value using the name and value collection of parameters.
        /// </summary>
        /// <param name="parameterNameValues">The collection containing the name and value of the parameters.</param>
        public object Resolve(NameValueCollection parameterNameValues)
        {
            if (parameterNameValues == null)
            {
                throw Error.ArgumentNull("parameterNameValues");
            }

            string parameterValue = parameterNameValues[Alias];
            if (parameterValue != null)
            {
                return ODataUriUtils.ConvertFromUriLiteral(parameterValue, ODataVersion.V4, EdmModel, EdmType);
            }

            return null;
        }
    }
}
