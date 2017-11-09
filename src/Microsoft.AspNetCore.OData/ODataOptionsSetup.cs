// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Sets up default options for <see cref="ODataOptions"/>.
    /// </summary>
    public class ODataOptionsSetup : IConfigureOptions<ODataOptions>
    {
        private IServiceProvider services;

        /// <summary>
        /// Initializes a new instance of the <see cref="ODataOptionsSetup"/> class.
        /// </summary>
        public ODataOptionsSetup(IServiceProvider services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            this.services = services;
        }

        /// <inheritdoc />
        public void Configure(ODataOptions options)
        {
        }
    }
}
