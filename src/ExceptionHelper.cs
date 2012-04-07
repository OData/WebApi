// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Internal.Web.Utils
{
    internal static class ExceptionHelper
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "Method may not be used in every assembly it is imported into")]
        internal static ArgumentException CreateArgumentNullOrEmptyException(string paramName)
        {
            return new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, paramName);
        }
    }
}
