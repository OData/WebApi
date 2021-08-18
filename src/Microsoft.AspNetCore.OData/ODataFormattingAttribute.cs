//-----------------------------------------------------------------------------
// <copyright file="ODataFormattingAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An attribute to be placed on controllers that enables the OData formatters.
    /// </summary>
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want to be able to subclass this type.")]
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ODataFormattingAttribute : Attribute
    {
        // This class is not needed; Formatters are injected in ODataServiceCollectionExtensions::AddOdata()
    }
}
