// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http.OData.Formatter.Deserialization
{
    public class ClrTypeAnnotation
    {
        public ClrTypeAnnotation(Type clrType)
        {
            ClrType = clrType;
        }

        public Type ClrType { get; set; }
    }
}
