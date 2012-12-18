// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter
{
    internal class ODataMetadataLevelHelper
    {
        public static bool IsDefined(ODataMetadataLevel value)
        {
            return value == ODataMetadataLevel.Default ||
                   value == ODataMetadataLevel.FullMetadata ||
                   value == ODataMetadataLevel.MinimalMetadata ||
                   value == ODataMetadataLevel.NoMetadata;
        }

        public static void Validate(ODataMetadataLevel value, string parameterValue)
        {
            if (!IsDefined(value))
            {
                throw Error.InvalidEnumArgument(parameterValue, (int)value, typeof(ODataMetadataLevel));
            }
        }
    }
}
