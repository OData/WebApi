// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing an unbound function invocation.
    /// </summary>
    public class UnboundFunctionPathSegment : ODataPathSegment
    {
        private IEdmModel _edmModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnboundFunctionPathSegment" /> class.
        /// </summary>
        /// <param name="function">The function being invoked.</param>
        /// <param name="model">The edm model containing the function.</param>
        /// <param name="parameterValues">The raw parameter values indexed by the parameter names.</param>
        public UnboundFunctionPathSegment(IEdmFunctionImport function, IEdmModel model, IDictionary<string, string> parameterValues)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

            if (model == null)
            {
                throw Error.ArgumentNull("model");
            }

            Function = function;
            FunctionName = Function.Name;
            _edmModel = model;
            Values = parameterValues ?? new Dictionary<string, string>();
        }

        // This constructor is intended for use by unit testing only.
        internal UnboundFunctionPathSegment(string functionName, IDictionary<string, string> parameterValues)
        {
            Contract.Assert(!String.IsNullOrEmpty(functionName));

            Values = parameterValues ?? new Dictionary<string, string>();
            FunctionName = functionName;
        }

        /// <summary>
        /// Gets the segment kind for the current segment.
        /// </summary>
        public override string SegmentKind
        {
            get
            {
                return ODataSegmentKinds.UnboundFunction;
            }
        }

        /// <summary>
        /// Gets the function being invoked.
        /// </summary>
        public IEdmFunctionImport Function { get; private set; }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string FunctionName { get; private set; }

        internal IDictionary<string, string> Values { get; private set; }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            // for unbound function, the previous Edm type must be null
            if (previousEdmType != null)
            {
                throw Error.Argument("previousEdmType");
            }

            if (Function != null)
            {
                IEdmTypeReference returnType = Function.Function.ReturnType;
                if (returnType != null)
                {
                    return returnType.Definition;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public override IEdmNavigationSource GetNavigationSource(IEdmNavigationSource previousNavigationSource)
        {
            // for unbound function, the previous navigation source must be null
            if (previousNavigationSource != null)
            {
                throw Error.Argument("previousNavigationSource");
            }

            if (Function != null)
            {
                // In current ODL V4 implementation, unbound function can't return the singleton.
                IEdmEntitySet functionEntitySet = null;
                if (Function.TryGetStaticEntitySet(out functionEntitySet))
                {
                    return functionEntitySet;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns>
        /// The value of the parameter.
        /// </returns>
        public object GetParameterValue(string parameterName)
        {
            if (String.IsNullOrEmpty(parameterName))
            {
                throw Error.ArgumentNullOrEmpty("parameterName");
            }

            string paramValue;
            if (Values.TryGetValue(parameterName, out paramValue))
            {
                IEdmOperationParameter edmParam = Function.Function.FindParameter(parameterName);
                if (edmParam != null)
                {
                    return ODataUriUtils.ConvertFromUriLiteral(paramValue, ODataVersion.V4, _edmModel, edmParam.Type);
                }
            }

            throw Error.Argument("parameterName", SRResources.FunctionParameterNotFound, parameterName);
        }

        /// <summary>
        /// Returns a <see cref="String" /> that represents this instance.
        /// </summary>
        /// <returns> a <see cref="String" /> to represent this instance. </returns>
        public override string ToString()
        {
            IEnumerable<string> parameters = Values.Select(v => String.Format(CultureInfo.InvariantCulture, "{0}={1}", v.Key, v.Value));
            return String.Format(CultureInfo.InvariantCulture, "{0}({1})", FunctionName, String.Join(",", parameters));
        }

        /// <inheritdoc />
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.UnboundFunction)
            {
                UnboundFunctionPathSegment functionSegment = (UnboundFunctionPathSegment)pathSegment;
                return functionSegment.Function == Function && functionSegment.FunctionName == FunctionName;
            }

            return false;
        }
    }
}
