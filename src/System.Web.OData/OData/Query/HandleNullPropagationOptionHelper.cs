// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Web.Http.OData.Query
{
    internal static class HandleNullPropagationOptionHelper
    {
        private const string EntityFrameworkQueryProviderNamespace = "System.Data.Entity.Internal.Linq";

        private const string ObjectContextQueryProviderNamespaceEF5 = "System.Data.Objects.ELinq";
        private const string ObjectContextQueryProviderNamespaceEF6 = "System.Data.Entity.Core.Objects.ELinq";

        private const string Linq2SqlQueryProviderNamespace = "System.Data.Linq";
        private const string Linq2ObjectsQueryProviderNamespace = "System.Linq";

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
                    options = HandleNullPropagationOption.False;
                    break;

                case Linq2ObjectsQueryProviderNamespace:
                default:
                    options = HandleNullPropagationOption.True;
                    break;
            }

            return options;
        }
    }
}