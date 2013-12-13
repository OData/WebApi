// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Core;
using Microsoft.OData.Core.UriParser;
using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Validation;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// An <see cref="ODataPathSegment"/> implementation representing an function invocation.
    /// </summary>
    public class FunctionPathSegment : ODataPathSegment
    {
        private IEdmModel _edmModel;

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionPathSegment" /> class.
        /// </summary>
        /// <param name="function">The function being invoked.</param>
        /// <param name="model">The edm model containing the function.</param>
        /// <param name="parameterValues">The raw parameter values indexed by the parameter names.</param>
        public FunctionPathSegment(IEdmFunctionImport function, IEdmModel model, IDictionary<string, string> parameterValues)
        {
            if (function == null)
            {
                throw Error.ArgumentNull("function");
            }

            Function = function;
            FunctionName = Function.Container.FullName() + "." + Function.Name;
            _edmModel = model;
            Values = parameterValues ?? new Dictionary<string, string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FunctionPathSegment" /> class.
        /// </summary>
        /// <param name="functionName">Name of the function.</param>
        /// <param name="parameterValues"></param>
        public FunctionPathSegment(string functionName, IDictionary<string, string> parameterValues)
        {
            if (functionName == null)
            {
                throw Error.ArgumentNull("functionName");
            }

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
        public IEdmFunctionImport Function { get; private set; }

        /// <summary>
        /// Gets the name of the function.
        /// </summary>
        public string FunctionName { get; private set; }

        /// <summary>
        /// Gets the parameter values.
        /// </summary>
        internal IDictionary<string, string> Values { get; private set; }

        /// <summary>
        /// Gets the EDM type for this segment.
        /// </summary>
        /// <param name="previousEdmType">The EDM type of the previous path segment.</param>
        /// <returns>
        /// The EDM type for this segment.
        /// </returns>
        public override IEdmType GetEdmType(IEdmType previousEdmType)
        {
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

        /// <summary>
        /// Gets the entity set for this segment.
        /// </summary>
        /// <param name="previousEntitySet">The entity set of the previous path segment.</param>
        /// <returns>
        /// The entity set for this segment.
        /// </returns>
        public override IEdmEntitySet GetEntitySet(IEdmEntitySet previousEntitySet)
        {
            if (Function != null)
            {
                // entity set
                IEdmEntitySet functionEntitySet = null;
                if (Function.TryGetStaticEntitySet(out functionEntitySet))
                {
                    return functionEntitySet;
                }
                // entity set path
                IEdmOperationParameter parameter;
                IEnumerable<IEdmNavigationProperty> path;
                IEnumerable<EdmError> edmErrors;
                if (Function.TryGetRelativeEntitySetPath(_edmModel, out parameter, out path, out edmErrors) &&
                    !edmErrors.Any())
                {
                    IEdmEntitySet entitySet = previousEntitySet;
                    foreach (IEdmNavigationProperty prop in path)
                    {
                        entitySet = entitySet.FindNavigationTarget(prop);
                    }
                    return entitySet;
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
                    return paramValue.StartsWith("@", StringComparison.Ordinal) ?
                        new UnresolvedParameterValue(edmParam.Type, paramValue, _edmModel) :
                        ODataUriUtils.ConvertFromUriLiteral(paramValue, ODataVersion.V4, _edmModel, edmParam.Type);
                }
            }

            throw Error.Argument("parameterName", SRResources.FunctionParameterNotFound, parameterName);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
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
                FunctionPathSegment functionSegment = (FunctionPathSegment)pathSegment;
                return functionSegment.Function == Function && functionSegment.FunctionName == FunctionName;
            }

            return false;
        }
    }
}
