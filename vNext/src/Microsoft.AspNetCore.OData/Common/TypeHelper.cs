using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.OData.Extensions;

namespace Microsoft.AspNetCore.OData.Common
{
    internal static class TypeHelper
    {
        /// <summary>
        /// Returns type of T if the type implements IEnumerable of T, otherwise, return null.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static Type GetImplementedIEnumerableType(Type type)
        {
            if (type == null)
            {
                throw Error.ArgumentNull("type");
            }

            foreach (var interfaceType in type.GetInterfaces().Concat(new[] { type}))
            {

                var gt = interfaceType.GetGenericArguments();

                if (gt.Count()==1)
                {
                    return gt[0];
                }
            }

            return null;
        }

        internal static IEnumerable<Type> GetLoadedTypes(string assemblyName)
        {
            var result = new List<Type>();

            // Go through all assemblies referenced by the application and search for types matching a predicate
	        var assemblies =
		        DefaultAssemblyPartDiscoveryProvider.DiscoverAssemblyParts(assemblyName)
			        .Select(s => (s as AssemblyPart).Assembly);
            foreach (var assembly in assemblies)
            {
                Type[] exportedTypes = null;
                if (assembly == null || assembly.IsDynamic)
                {
                    // can't call GetTypes on a null (or dynamic?) assembly
                    continue;
                }

                try
                {
                    exportedTypes = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    exportedTypes = ex.Types;
                }
                catch
                {
                    continue;
                }

                if (exportedTypes != null)
                {
                    result.AddRange(exportedTypes.Where(t => t != null && t.GetTypeInfo().IsVisible));
                }
            }

            return result;
        }

        public static Type GetUnderlyingTypeOrSelf(Type type)
        {
            return Nullable.GetUnderlyingType(type) ?? type;
        }

        public static bool IsEnum(Type type)
        {
            Type underlyingTypeOrSelf = GetUnderlyingTypeOrSelf(type);
            return underlyingTypeOrSelf.GetTypeInfo().IsEnum;
        }

		public static Type GetInnerElementType(this Type type)
		{
			Type elementType;
			type.IsCollection(out elementType);
			Contract.Assert(elementType != null);

			return elementType;
		}
	}
}