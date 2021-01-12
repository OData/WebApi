// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Represents a type of a <see cref="DataModificationException"/> class
    /// </summary>
    public class DataModificationException
    {
        /// <summary>
        /// Creates an instance of <see cref="DataModificationException"/>
        /// </summary>
        public DataModificationException()
        {
            InstanceAnnotationContainer = new ODataInstanceAnnotationContainer();
        }

        /// <summary>
        /// Holds Instane Annotations
        /// </summary>
        public IODataInstanceAnnotationContainer InstanceAnnotationContainer { get; set; }
    }
}