// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter
{
    internal static class PatchKeyModeHelper
    {
        public static bool IsDefined(PatchKeyMode patchKeyMode)
        {
            return patchKeyMode == PatchKeyMode.Ignore ||
                patchKeyMode == PatchKeyMode.Patch ||
                patchKeyMode == PatchKeyMode.Throw;
        }

        public static void Validate(PatchKeyMode value, string parameterName)
        {
            if (!IsDefined(value))
            {
                throw Error.InvalidEnumArgument(parameterName, (int)value, typeof(PatchKeyMode));
            }
        }
    }
}
