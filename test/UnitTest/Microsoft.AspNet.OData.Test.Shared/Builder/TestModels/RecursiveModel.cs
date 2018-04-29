// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    public class RecursivePropertyContainer
    {
        public Guid Id { get; set; }

        public GenericSurrogate Item { get; set; }
    }

    public abstract class GenericSurrogate
    {
    }

    public class MyExpression : GenericSurrogate
    {
        public GenericSurrogate Item { get; set; }
    }
}
