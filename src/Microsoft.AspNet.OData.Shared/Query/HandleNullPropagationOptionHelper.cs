// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Query
{
    internal static class HandleNullPropagationOptionHelper
    {
        internal const string EntityFrameworkQueryProviderNamespace = "System.Data.Entity.Internal.Linq";

        internal const string ObjectContextQueryProviderNamespaceEF5 = "System.Data.Objects.ELinq";
        internal const string ObjectContextQueryProviderNamespaceEF6 = "System.Data.Entity.Core.Objects.ELinq";
        internal const string ObjectContextQueryProviderNamespaceEFCore2 = "Microsoft.EntityFrameworkCore.Query.Internal";

        internal const string Linq2SqlQueryProviderNamespace = "System.Data.Linq";
        internal const string Linq2ObjectsQueryProviderNamespace = "System.Linq";

        public static bool IsDefined(HandleNullPropagationOption value)
        {
            return value == HandleNullPropagationOption.Default ||
                   value == HandleNullPropagationOption.True ||
                   value == HandleNullPropagationOption.False;
        }

        public static void Validate(HandleNullPropagationOption value, string parameterValue)
        {
            if (!IsDefined(value))
            {
                throw Error.InvalidEnumArgument(parameterValue, (int)value, typeof(HandleNullPropagationOption));
            }
        }

        public static HandleNullPropagationOption GetDefaultHandleNullPropagationOption(IQueryable query)
        {
            Contract.Assert(query != null);

            HandleNullPropagationOption options;

            string queryProviderNamespace = query.Provider.GetType().Namespace;
            switch (queryProviderNamespace)
            {
                case EntityFrameworkQueryProviderNamespace:
                case Linq2SqlQueryProviderNamespace:
                case ObjectContextQueryProviderNamespaceEF5:
                case ObjectContextQueryProviderNamespaceEF6:

                // EF Core before 3.0 does a lot of client evaluations and has InMemory support which is the same as Linq2Objects
                // We have to keep the null propagation for EF Core 3.0, otherwise, there's some issues for example:
                // $expand=Collection, $select=Collection
                case ObjectContextQueryProviderNamespaceEFCore2:
                    options = HandleNullPropagationOption.False;
                    break;

                case Linq2ObjectsQueryProviderNamespace:

                default:
                    options = HandleNullPropagationOption.True;
                    break;
            }

            return options;
        }

        public static DataSourceProviderKind GetDataSourceProviderKind(this IQueryable query)
        {
            string queryProviderNamespace = query.Provider.GetType().Namespace;
            switch (queryProviderNamespace)
            {
                case EntityFrameworkQueryProviderNamespace:
                case Linq2SqlQueryProviderNamespace:
                case ObjectContextQueryProviderNamespaceEF5:
                case ObjectContextQueryProviderNamespaceEF6:
                    return DataSourceProviderKind.EFClassic;

                case ObjectContextQueryProviderNamespaceEFCore2:
                    return DataSourceProviderKind.EFCore;

                default:
                    return DataSourceProviderKind.InMemory;
            }
        }
    }

    internal enum DataSourceProviderKind
    {
        /// <summary>
        /// None Data source provider
        /// </summary>
        None,

        /// <summary>
        /// In memory data source provider
        /// </summary>
        InMemory,

        /// <summary>
        /// EF classic Data source provider
        /// </summary>
        EFClassic,

        /// <summary>
        /// EF Core Data source provider
        /// </summary>
        EFCore
    }
}