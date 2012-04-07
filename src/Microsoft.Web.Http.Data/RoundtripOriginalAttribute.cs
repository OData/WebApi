// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// When applied to a member, this attribute indicates that the original value of
    /// the member should be sent back to the server when the object is updated. When applied
    /// to a class, the attribute gets applied to each member of that class. If this attribute is not 
    /// present, the value of this member will be null in the original object sent back to the server.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class RoundtripOriginalAttribute : Attribute
    {
    }
}
