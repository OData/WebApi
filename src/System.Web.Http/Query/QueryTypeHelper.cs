using System.Diagnostics.Contracts;
using System.Linq;

namespace System.Web.Http.Query
{
    /// <summary>
    /// A static class that provides Queryable related types and functionality
    /// to perform checks related on them.
    /// </summary>
    internal static class QueryTypeHelper
    {
        private static readonly Type _queryableInterfaceGenericType = typeof(IQueryable<>);

        internal static Type GetQueryableInterfaceInnerTypeOrNull(Type type)
        {
            if (type == null)
            {
                return type;
            }

            if (IsQueryableInterfaceGenericType(type))
            {
                return type.GetGenericArguments()[0];
            }
            else if (ImplementsQueryableInterfaceGenericType(type))
            {
                return type.GetInterface(_queryableInterfaceGenericType.FullName).GetGenericArguments()[0];
            }

            return null;
        }

        private static bool IsQueryableInterfaceGenericType(Type type)
        {
            Contract.Assert(type != null);

            return type.IsInterface
                   && type.IsGenericType
                   && type.GetGenericTypeDefinition().Equals(_queryableInterfaceGenericType);
        }

        private static bool ImplementsQueryableInterfaceGenericType(Type type)
        {
            Contract.Assert(type != null);

            return type.GetInterface(_queryableInterfaceGenericType.FullName) != null;
        }
    }
}
