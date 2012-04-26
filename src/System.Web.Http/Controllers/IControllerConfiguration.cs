// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Web.Http.Controllers
{
    /// <summary>
    /// If a controller is decorated with an attribute with this interface, than it gets invoked
    /// to initialize the controller descriptor. 
    /// </summary>
    public interface IControllerConfiguration
    {
        /// <summary>
        /// Callback invoked to set per-controller overrides for this controllerDescriptor.
        /// </summary>
        /// <param name="controllerDescriptor">controller configuration to initialize</param>
        void Initialize(HttpControllerDescriptor controllerDescriptor);
    }
}
