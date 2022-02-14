//-----------------------------------------------------------------------------
// <copyright file="ModelAlias.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

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
