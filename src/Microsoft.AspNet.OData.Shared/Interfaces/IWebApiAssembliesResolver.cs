﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// Provides an abstraction for managing the assemblies of an application.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be exposed publicly; it used for the internal
    /// implementations of SelectControl(). Any design which makes this class public
    /// should find an alternative design.
    /// </remarks>
    internal interface IWebApiAssembliesResolver
    {
        /// <summary>
        /// Gets a list of assemblies available for the application.
        /// </summary>
        /// <returns>A list of assemblies available for the application. </returns>
        IEnumerable<Assembly> Assemblies { get; }
    }
}
