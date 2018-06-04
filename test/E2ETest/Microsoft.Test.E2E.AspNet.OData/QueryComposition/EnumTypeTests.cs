// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.


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
