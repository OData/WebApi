//-----------------------------------------------------------------------------
// <copyright file="Level.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Runtime.Serialization;

namespace AspNetCoreODataSample.Web.Models
{
    [DataContract(Name = "level")]
    public enum Level
    {
        [EnumMember(Value = "low")]
        Low,

        [EnumMember(Value = "medium")]
        Medium,

        [EnumMember(Value = "veryhigh")]
        High
    }
}
