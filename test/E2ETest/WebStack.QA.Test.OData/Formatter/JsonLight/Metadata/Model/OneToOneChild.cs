// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

namespace WebStack.QA.Test.OData.Formatter.JsonLight.Metadata.Model
{
    public class OneToOneChild
    {
        public int Id { get; set; }
        public OneToOneParent Parent { get; set; }
    }
}
