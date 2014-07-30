// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing a bound function invocation.
    /// </summary>
    public class BoundFunctionPathSegment : ODataPathSegment
    {
        private IEdmModel _edmModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="BoundFunctionPathSegment" /> class.
        /// </summary>
        /// <param name="function">The function being invoked.</param>
        /// <param name="model">The edm model containing the function.</param>
        /// <param name="parameterValues">The raw parameter values indexed by the parameter names.</param>
        public BoundFunctionPathSegment(IEdmFunction function, IEdmModel model, IDictionary<string, string> parameterValues)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

            Function = function;
            FunctionName = Function.FullName();
            _edmModel = model;
            Values = parameterValues ?? new Dictionary<string, string>();
        }

        internal BoundFunctionPathSegment(string functionName, IDictionary<string, string> parameterValues)
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
                return ODataSegmentKinds.Function;
            }
        }

        /// <summary>
        /// Gets the function being invoked.
        /// </summary>
        public IEdmFunction Function { get; private set; }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string FunctionName { get; private set; }

        internal IDictionary<string, string> Values { get; private set; }

        /// <inheritdoc/>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
            if (Function != null)
            {
                IEdmTypeReference returnType = Function.ReturnType;
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
            if (_edmModel == null)
            {
                return null;
            }

            // Try to use the entity set annotation to get the target navigation source.
            ReturnedEntitySetAnnotation entitySetAnnotation =
                    _edmModel.GetAnnotationValue<ReturnedEntitySetAnnotation>(Function);

            if (entitySetAnnotation != null)
            {
                return _edmModel.EntityContainer.FindEntitySet(entitySetAnnotation.EntitySetName);
            }

            // Try to use the entity set path to get the target navigation source.
            // An entity set path is a string list seperated by '/', for example "bindingParameter/Orders".
            // The first segment must be the binding parameter name, while the orthers must be the navigation property
            // name. ODL can use the entity set path expression and the bound navigation source to calucate the target
            // navigation source. for example:
            // ~/CustomersA(1)/GetRelatedOrders() will return OrdersA
            // ~/CustomersB(1)/GetRelatedOrders() will return OrdersB
            if (previousNavigationSource != null && _edmModel != null && Function != null)
            {
                IEdmOperationParameter parameter;
                IEnumerable<IEdmNavigationProperty> navigationProperties;
                IEdmEntityType lastEntityType;
                IEnumerable<EdmError> errors;

                if (Function.TryGetRelativeEntitySetPath(_edmModel, out parameter, out navigationProperties,
                    out lastEntityType, out errors))
                {
                    IEdmNavigationSource targetNavigationSource = previousNavigationSource;
                    foreach (IEdmNavigationProperty navigationProperty in navigationProperties)
                    {
                        targetNavigationSource = targetNavigationSource.FindNavigationTarget(navigationProperty);
                        if (targetNavigationSource == null)
                        {
                            return null;
                        }
                    }

                    return targetNavigationSource;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the parameter value.
        /// </summary>
        /// <param name="parameterName">The name of the parameter.</param>
        /// <returns> The value of the parameter. </returns>
        public object GetParameterValue(string parameterName)
        {
            if (String.IsNullOrEmpty(parameterName))
            {
                throw Error.ArgumentNullOrEmpty("parameterName");
            }

            string paramValue;
            if (Values.TryGetValue(parameterName, out paramValue))
            {
                IEdmOperationParameter edmParam = Function.FindParameter(parameterName);
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
        /// <returns>
        /// A <see cref="String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            IEnumerable<string> parameters = Values.Select(v => String.Format(CultureInfo.InvariantCulture, "{0}={1}", v.Key, v.Value));
            return String.Format(CultureInfo.InvariantCulture, "{0}({1})", FunctionName, String.Join(",", parameters));
        }

        /// <inheritdoc />
        public override bool TryMatch(ODataPathSegment pathSegment, IDictionary<string, object> values)
        {
            if (pathSegment.SegmentKind == ODataSegmentKinds.Function)
            {
                BoundFunctionPathSegment functionSegment = (BoundFunctionPathSegment)pathSegment;
                return functionSegment.Function == Function && functionSegment.FunctionName == FunctionName;
            }

            return false;
        }
    }
}
