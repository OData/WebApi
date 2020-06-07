// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Factory for <see cref="IODataQueryOptionsParser"/> classes to handle parsing of OData query options in the request body.
    /// </summary>
    public static class ODataQueryOptionsParserFactory
    {
        /// <summary>
        /// Creates a list of <see cref="IODataQueryOptionsParser"/>s to handle parsing of OData query options in the request body.
        /// </summary>
        /// <returns>A list of <see cref="IODataQueryOptionsParser"/>s to handle parsing of OData query options in the request body.</returns>
        public static IList<IODataQueryOptionsParser> Create()
        {
            Type interfaceType = typeof(IODataQueryOptionsParser);
            // Find all types that implement IODataQueryOptionsParser interface and have a parameterless constructor
            var implementingTypes = TypeHelper.GetLoadedTypes(WebApiAssembliesResolver.Default).Where(type => type.IsClass &&
                !type.IsAbstract &&
                interfaceType.IsAssignableFrom(type) &&
                type.GetConstructor(new Type[0]) != null);
            
            IList<IODataQueryOptionsParser> parsers = new List<IODataQueryOptionsParser>();

            // Create an instance of each implementing type
            foreach(var type in implementingTypes)
            {
                parsers.Add((IODataQueryOptionsParser)Activator.CreateInstance(type));
            }

            return parsers;
        }
    }
}
