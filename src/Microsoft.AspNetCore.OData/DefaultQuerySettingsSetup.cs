// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNet.OData
{
    /// <summary>
    /// Sets up default options for <see cref="DefaultQuerySettings"/>.
    /// </summary>
    public class DefaultQuerySettingsSetup : IConfigureOptions<DefaultQuerySettings>
    {
        private IServiceProvider services;

        public DefaultQuerySettingsSetup(IServiceProvider services)
        {
            if (services == null)
            {
                throw Error.ArgumentNull("services");
            }

            this.services = services;
        }

        public void Configure(DefaultQuerySettings options)
        {
            // DefaultQuerySettings requires no additional configuration.
        }
    }
}
