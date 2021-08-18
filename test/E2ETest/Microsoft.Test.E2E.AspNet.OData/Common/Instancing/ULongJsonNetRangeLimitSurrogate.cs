//-----------------------------------------------------------------------------
// <copyright file="ULongJsonNetRangeLimitSurrogate.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    // Since Json.NET cannot deserialize anything longer than Int64,
    // this surrgoate is necessary to create ulong value that is roundtrippable
    // http://json.codeplex.com/workitem/22320
    public class ULongJsonNetRangeLimitSurrgate : InstanceCreatorSurrogate
    {
        public override bool CanCreateInstanceOf(Type type)
        {
            return (type == typeof(ulong));
        }

        public override object CreateInstanceOf(Type type, Random rndGen, CreatorSettings creatorSettings)
        {
            ulong ul;
            do
            {
                ul = PrimitiveCreator.CreateInstanceOfUInt64(rndGen, creatorSettings);
            }
            while (ul > long.MaxValue);

            return ul;
        }
    }
}
