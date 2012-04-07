// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Security;

namespace System.Web.Http.Internal
{
    // Note: DataAnnotations on 4.0 should have been a transparent assembly but is not. As a result 
    // we need to mark this as SecuritySafeCritical to run outside ASP (which has special knowledge
    // about the DataAnnotations assembly).
    [SecuritySafeCritical]
    internal static class TypeDescriptorHelper
    {
        internal static ICustomTypeDescriptor Get(Type type)
        {
            return new AssociatedMetadataTypeTypeDescriptionProvider(type).GetTypeDescriptor(type);
        }
    }
}
