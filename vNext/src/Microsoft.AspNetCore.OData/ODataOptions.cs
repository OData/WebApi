// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.OData.Abstracts;
using Microsoft.AspNetCore.OData.Routing.Conventions;
using Microsoft.OData;

namespace Microsoft.AspNetCore.OData
{
    /// <summary>
    /// Provides programmatic configuration for the OData service.
    /// </summary>
    public class ODataOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOptions"/> class.
        /// </summary>
        public ODataOptions()
        {
            //Model = null;
            RoutingConventions = new List<IODataRoutingConvention>();
            ModelManager = new ODataModelManager();
        }

        public IODataModelManger ModelManager { get; private set; }

        /// <summary>
        /// Gets or Sets the <see cref="ODataUrlKeyDelimiter"/> to use while parsing, specifically
        /// whether to recognize keys as segments or not.
        /// </summary>
        public ODataUrlKeyDelimiter UrlKeyDelimiter { get; set; }

        /// <summary>
        /// Gets or sets a collection of <see cref="IODataRoutingConvention"/> which are used to routing.
        /// </summary>
        public IList<IODataRoutingConvention> RoutingConventions { get; set; }

        // TODO: and more configuration here.
    }
}
