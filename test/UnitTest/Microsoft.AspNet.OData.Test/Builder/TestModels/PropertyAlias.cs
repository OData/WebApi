// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace Microsoft.Test.AspNet.OData.Builder.TestModels
{
    [DataContract(Namespace = "com.contoso", Name = "PropertyAlias2")]
    public class PropertyAlias
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember(Name = "FirstNameAlias")]
        public string FirstName { get; set; }

        public int Points { get; set; }
    }

    [DataContract(Namespace = "com.contoso", Name = "PropertyAliasDerived2")]
    public class PropertyAliasDerived : PropertyAlias
    {
        [DataMember(Name = "LastNameAlias")]
        public string LastName { get; set; }
    
        public int Age { get; set; }
    }
}
