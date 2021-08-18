//-----------------------------------------------------------------------------
// <copyright file="PasswordFormat.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Nop.Core.Domain.Customers
{
    public enum PasswordFormat : int
    {
        Clear = 0,
        Hashed = 1,
        Encrypted = 2
    }
}
