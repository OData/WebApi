// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Query
{
    internal static class HandleNullPropagationOptionHelper
    {
        public static bool IsDefined(HandleNullPropagationOption value)
        {
            return value == HandleNullPropagationOption.Default ||
                   value == HandleNullPropagationOption.True ||
                   value == HandleNullPropagationOption.False;
        }

        public static void Validate(HandleNullPropagationOption value, string parameterValue)
        {
            if (!IsDefined(value))
            {
                throw Error.InvalidEnumArgument(parameterValue, (int)value, typeof(HandleNullPropagationOption));
            }
        }
    }
}