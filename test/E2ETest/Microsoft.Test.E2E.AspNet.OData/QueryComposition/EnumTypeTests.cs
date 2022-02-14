//-----------------------------------------------------------------------------
// <copyright file="EnumTypeTests.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.Test.E2E.AspNet.OData.QueryComposition
{
    public enum EnumType_Type
    {
        Task,
        Reminder
    }
    public class EnumType_Todo
    {
        public int ID { get; set; }
        public EnumType_Type Type { get; set; }
    }

}
