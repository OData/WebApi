// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace System.Web.OData.Builder.TestModels
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
