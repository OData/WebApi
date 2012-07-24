// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Runtime.Serialization;

namespace System.Net.Http.Formatting.DataSets.Types
{
    [DataContract]
    public enum DataContractEnum
    {
        [EnumMember]
        First,

        [EnumMember]
        Second,

        Third
    }
}
