// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace Microsoft.AspNet.OData.Interfaces
{
    /// <summary>
    /// An interface used to search for an available action.
    /// </summary>
    /// <remarks>
    /// This class is not intended to be exposed publicly; it used for the internal
    /// implementations of SelectControl(). Any design which makes this class public
    /// should find an alternative design.
    /// </remarks>
    internal interface IWebApiActionMap
    {
        /// <summary>
        /// Determines whether a specified key exists.
        /// </summary>
        /// <param name="name">The key.</param>
        /// <returns>True if the key exist; false otherwise.</returns>
        bool Contains(string name);

        /// <summary>
        /// Gets the action descriptor of the specified action
        /// </summary>
        /// <param name="actionName">The name of the action</param>
        /// <returns>The <see cref="IWebApiActionDescriptor"/> if it exists, otherwise null</returns>
        IWebApiActionDescriptor GetActionDescriptor(string actionName);
    }
}
