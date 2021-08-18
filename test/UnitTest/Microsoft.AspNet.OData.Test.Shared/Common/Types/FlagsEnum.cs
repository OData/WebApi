//-----------------------------------------------------------------------------
// <copyright file="FlagsEnum.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;

namespace Microsoft.AspNet.OData.Test.Common.Types
{
    [Flags]
    public enum FlagsEnum
    {
        One = 0x1,
        Two = 0x2,
        Four = 0x4
    }
}
