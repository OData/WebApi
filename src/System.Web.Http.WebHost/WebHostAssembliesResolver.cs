// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Compilation;
using System.Web.Http.Dispatcher;

namespace System.Web.Http.WebHost
{
    /// <summary>
    /// Provides an implementation of <see cref="IAssembliesResolver"/> using <see cref="BuildManager"/>.
    /// </summary>
    internal sealed class WebHostAssembliesResolver : IAssembliesResolver
    {
        /// <summary>
        /// Returns a list of controllers available for the application.
        /// </summary>
        /// <returns>An <see cref="ICollection{Assembly}" /> of controllers.</returns>
        ICollection<Assembly> IAssembliesResolver.GetAssemblies()
        {
            return BuildManager.GetReferencedAssemblies().OfType<Assembly>().ToList();
        }
    }
}
