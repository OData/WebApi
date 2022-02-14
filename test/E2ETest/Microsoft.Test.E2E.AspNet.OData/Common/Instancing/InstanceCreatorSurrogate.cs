//-----------------------------------------------------------------------------
// <copyright file="InstanceCreatorSurrogate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    /// <summary>
    /// Enables tests to create specific instances of certain types.
    /// </summary>
    public abstract class InstanceCreatorSurrogate
    {
        /// <summary>
        /// Checks whether this surrogate can create instances of a given type.
        /// </summary>
        /// <param name="type">The type which needs to be created.</param>
        /// <returns>A true value if this surrogate can create the given type; a false value otherwise.</returns>
        public abstract bool CanCreateInstanceOf(Type type);

        /// <summary>
        /// Creates an instance of the given type.
        /// </summary>
        /// <param name="type">The type to create an instance for.</param>
        /// <param name="rndGen">A Random generator to assist in creating the instance.</param>
        /// <param name="creatorSettings">The settings used to create objects.</param>
        /// <returns>An instance of the given type.</returns>
        /// <remarks>On implementations of this method, <b>do not call the InstanceCreator for objects
        /// of the type directly</b>. This will cause a stack overflow.</remarks>
        public abstract object CreateInstanceOf(Type type, Random rndGen, CreatorSettings creatorSettings);
    }
}
