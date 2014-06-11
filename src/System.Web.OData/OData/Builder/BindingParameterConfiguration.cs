// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Edm;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents a BindingParameter.
    /// <remarks>
    /// Actions/Functions can have at most one BindingParameter.
    /// This parameter has similar semantics to the 'this' keyword in C# extensions methods.
    /// <example>
    /// For example given a url that identifies a Movie, if there is an action that has a bindingParameter that is a Movie,
    /// you can bind the Action to the url.
    /// 
    /// i.e. if ~/Movies(1) identifies a Movie, and there exists a Checkout action that has a Movie BindingParameter,
    /// you can invoke that Action at this url ~/Movies(1)/Checkout
    /// </example>
    /// The BindingParameter type must either be an EntityType or a Collection of EntityTypes.
    /// </remarks>
    /// </summary>
    public class BindingParameterConfiguration : ParameterConfiguration
    {
        /// <summary>
        /// The default parameter name for an action's binding parameter.
        /// </summary>
        public const string DefaultBindingParameterName = "bindingParameter";

        /// <summary>
        /// Create a BindingParameterConfiguration
        /// </summary>
        /// <param name="name">The name of the Binding Parameter</param>
        /// <param name="parameterType">The type of the Binding Parameter</param>
        public BindingParameterConfiguration(string name, IEdmTypeConfiguration parameterType)
            : base(name, parameterType)
        {
            EdmTypeKind kind = parameterType.Kind;
            if (kind == EdmTypeKind.Collection)
            {
                kind = (parameterType as CollectionTypeConfiguration).ElementType.Kind;
            }
            if (kind != EdmTypeKind.Entity)
            {
                throw Error.Argument("parameterType", SRResources.InvalidBindingParameterType, parameterType.FullName);
            }
        }
    }
}
