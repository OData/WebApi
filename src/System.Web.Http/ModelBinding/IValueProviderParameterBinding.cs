// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.ValueProviders;

namespace System.Web.Http.ModelBinding
{
    /// <summary>
    /// Describes a parameter binding that uses one or more instances of <see cref="ValueProviderFactory"/>
    /// </summary>
    public interface IValueProviderParameterBinding
    {
        /// <summary>
        /// Gets the <see cref="ValueProviderFactory"/> instances used by this
        /// parameter binding.
        /// </summary>
        IEnumerable<ValueProviderFactory> ValueProviderFactories { get; }
    }
}
