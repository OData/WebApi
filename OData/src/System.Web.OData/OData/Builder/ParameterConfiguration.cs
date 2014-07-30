// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Http;

namespace System.Web.OData.Builder
{
    /// <summary>
    /// Represents a parameter to a Procedure
    /// </summary>
    public abstract class ParameterConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ParameterConfiguration"/> class.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="parameterType">The EDM type of the paramter.</param>
        protected ParameterConfiguration(string name, IEdmTypeConfiguration parameterType)
        {
            if (name == null)
            {
                throw Error.ArgumentNull("name");
            }
            if (parameterType == null)
            {
                throw Error.ArgumentNull("bindingParameterType");
            }
            Name = name;
            TypeConfiguration = parameterType;
        }

        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The type of the parameter
        /// </summary>
        public IEdmTypeConfiguration TypeConfiguration { get; protected set; }
    }
}
