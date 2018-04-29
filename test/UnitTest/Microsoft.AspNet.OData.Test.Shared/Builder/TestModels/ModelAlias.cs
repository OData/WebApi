// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace Microsoft.AspNet.OData.Test.Builder.TestModels
{
    [DataContract(Namespace = "com.contoso", Name = "ModelAlias2")]
    public class ModelAlias
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string FirstName { get; set; }
    }
}
