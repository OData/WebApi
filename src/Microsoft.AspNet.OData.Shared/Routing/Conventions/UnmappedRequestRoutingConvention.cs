﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Interfaces;

namespace Microsoft.AspNet.OData.Routing.Conventions
{
    /// <summary>
    /// An implementation of <see cref="IODataRoutingConvention"/> that always selects the action named HandleUnmappedRequest if that action is present.
    /// </summary>
    public partial class UnmappedRequestRoutingConvention : NavigationSourceRoutingConvention
    {
        private const string UnmappedRequestActionName = "HandleUnmappedRequest";

        /// <inheritdoc/>
        internal static string SelectActionImpl(IWebApiActionMap actionMap)
        {
            if (actionMap.Contains(UnmappedRequestActionName))
            {
                return UnmappedRequestActionName;
            }

            return null;
        }
    }
}
