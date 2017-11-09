// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// An attribute to be placed on controllers that enables the OData formatters.
    /// </summary>
    public class ODataFormattingAttribute : Attribute
    {
        // This class is not needed; Formatters are injected in ODataServiceCollectionExtensions::AddOdata()
    }
}
