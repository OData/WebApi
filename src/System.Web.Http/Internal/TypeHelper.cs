using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Common;
using System.Web.Http.Controllers;
using System.Web.Http.Properties;

namespace System.Web.Http.Internal
{
    /// <summary>
    /// A static class that provides various <see cref="Type"/> related helpers.
    /// </summary>
    internal static class TypeHelper
    {
        private static readonly Type HttpContentType = typeof(HttpContent);
        private static readonly Type HttpRequestMessageType = typeof(HttpRequestMessage);
        private static readonly Type HttpResponseMessageType = typeof(HttpResponseMessage);
        private static readonly Type ObjectContentGenericType = typeof(ObjectContent<>);
        private static readonly Type TaskGenericType = typeof(Task<>);

        internal static readonly Type HttpControllerType = typeof(IHttpController);
        internal static readonly Type ResponseMessageConverterType = typeof(Func<object, HttpControllerContext, HttpResponseMessage>);
        internal static readonly Type ApiControllerType = typeof(ApiController);

        internal static bool IsHttpContent(Type type)
        {
            Contract.Assert(type != null);
            return HttpContentType.IsAssignableFrom(type);
        }

        internal static bool IsHttpResponse(Type type)
        {
            Contract.Assert(type != null);
            return HttpResponseMessageType.IsAssignableFrom(type);
        }

        internal static bool IsHttpRequest(Type type)
        {
            Contract.Assert(type != null);
            return HttpRequestMessageType.IsAssignableFrom(type);
        }

        internal static bool IsHttpResponseOrContent(Type type)
        {
            Contract.Assert(type != null);
            return HttpContentType.IsAssignableFrom(type) ||
                   HttpResponseMessageType.IsAssignableFrom(type);
        }

        internal static bool IsHttpRequestOrContent(Type type)
        {
            Contract.Assert(type != null);
            return HttpContentType.IsAssignableFrom(type) ||
                   HttpRequestMessageType.IsAssignableFrom(type);
        }

        internal static bool IsHttp(Type type)
        {
            Contract.Assert(type != null);
            return HttpContentType.IsAssignableFrom(type) ||
                   HttpRequestMessageType.IsAssignableFrom(type) ||
                   HttpResponseMessageType.IsAssignableFrom(type);
        }

        internal static bool IsHttpContentGenericTypeDefinition(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericTypeDefinition && ObjectContentGenericType.IsAssignableFrom(type))
            {
                return true;
            }

            return false;
        }

        internal static bool IsHttpRequestOrContentGenericTypeDefinition(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericTypeDefinition)
            {
                if (ObjectContentGenericType.IsAssignableFrom(type))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsHttpResponseOrContentGenericTypeDefinition(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericTypeDefinition)
            {
                if (ObjectContentGenericType.IsAssignableFrom(type))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsHttpGenericTypeDefinition(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericTypeDefinition)
            {
                if (ObjectContentGenericType.IsAssignableFrom(type))
                {
                    return true;
                }
            }

            return false;
        }

        internal static Type GetHttpContentInnerTypeOrNull(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (IsHttpContentGenericTypeDefinition(genericTypeDefinition))
                {
                    Type[] typeArgs = type.GetGenericArguments();
                    if (typeArgs.Length > 1)
                    {
                        throw Error.InvalidOperation(SRResources.MultipleTypeParametersForHttpContentType, type.Name);
                    }

                    return typeArgs[0];
                }
            }

            return null;
        }

        internal static Type GetHttpRequestOrContentInnerTypeOrNull(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (IsHttpRequestOrContentGenericTypeDefinition(genericTypeDefinition))
                {
                    Type[] typeArgs = type.GetGenericArguments();
                    if (typeArgs.Length > 1)
                    {
                        throw Error.InvalidOperation(SRResources.MultipleTypeParametersForHttpContentType, type.Name);
                    }

                    return typeArgs[0];
                }
            }

            return null;
        }

        internal static Type GetHttpResponseOrContentInnerTypeOrNull(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                if (IsHttpResponseOrContentGenericTypeDefinition(genericTypeDefinition))
                {
                    Type[] typeArgs = type.GetGenericArguments();
                    if (typeArgs.Length > 1)
                    {
                        throw Error.InvalidOperation(SRResources.MultipleTypeParametersForHttpContentType, type.Name);
                    }

                    return typeArgs[0];
                }
            }

            return null;
        }

        internal static Type GetTaskInnerTypeOrNull(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                // REVIEW: should we consider subclasses of Task<> ??
                if (TaskGenericType == genericTypeDefinition)
                {
                    return type.GetGenericArguments()[0];
                }
            }

            return null;
        }

        internal static Type GetUnderlyingContentInnerType(Type type)
        {
            Contract.Assert(type != null);

            Type httpResponseMessageType = TypeHelper.GetTaskInnerTypeOrNull(type) ?? type;
            Type contentType = TypeHelper.GetHttpResponseOrContentInnerTypeOrNull(httpResponseMessageType) ?? httpResponseMessageType;
            return contentType;
        }

        internal static Type MakeObjectContentOf(Type type)
        {
            Contract.Assert(type != null);
            Type[] typeParams = new Type[] { type };
            return TypeHelper.ObjectContentGenericType.MakeGenericType(typeParams);
        }

        internal static Type ExtractGenericInterface(Type queryType, Type interfaceType)
        {
            Func<Type, bool> matchesInterface = t => t.IsGenericType && t.GetGenericTypeDefinition() == interfaceType;
            return matchesInterface(queryType) ? queryType : queryType.GetInterfaces().FirstOrDefault(matchesInterface);
        }

        internal static Type[] GetTypeArgumentsIfMatch(Type closedType, Type matchingOpenType)
        {
            if (!closedType.IsGenericType)
            {
                return null;
            }

            Type openType = closedType.GetGenericTypeDefinition();
            return (matchingOpenType == openType) ? closedType.GetGenericArguments() : null;
        }

        internal static bool IsCompatibleObject(Type type, object value)
        {
            return (value == null && TypeAllowsNullValue(type)) || type.IsInstanceOfType(value);
        }

        internal static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        internal static bool TypeAllowsNullValue(Type type)
        {
            return !type.IsValueType || IsNullableValueType(type);
        }

        internal static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.Equals(typeof(string)) ||
                   type.Equals(typeof(DateTime)) ||
                   type.Equals(typeof(Decimal)) ||
                   type.Equals(typeof(Guid)) ||
                   type.Equals(typeof(DateTimeOffset)) ||
                   type.Equals(typeof(TimeSpan));
        }

        internal static bool IsSimpleUnderlyingType(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            return TypeHelper.IsSimpleType(type);
        }

        internal static bool HasStringConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

        internal static IEnumerable GetAsEnumerable(object o)
        {
            // string implements IEnumerable<char>, but we want to treat it as a primitive type. 
            if (o.GetType() == typeof(string))
            {
                return null;
            }
            return o as IEnumerable;
        }

        /// <summary>
        /// Determines whether the given type is a generic "http intrinsic"
        /// type, such as <see cref="ObjectContent{T}"/>.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns><c>true</c> if the type is a generic http intrinsic type.</returns>
        internal static bool IsGenericIntrinsicHttpType(Type type)
        {
            Contract.Assert(type != null);
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();
                return IsHttpRequestOrContentGenericTypeDefinition(genericTypeDefinition);
            }

            return false;
        }

        /// <summary>
        /// Fast implementation to get the subset of a given type.
        /// </summary>
        /// <typeparam name="T">type to search for</typeparam>
        /// <returns>subset of objects that can be assigned to T</returns>
        internal static ReadOnlyCollection<T> OfType<T>(object[] objects) where T : class
        {
            int max = objects.Length;
            List<T> list = new List<T>(max);
            int idx = 0;
            for (int i = 0; i < max; i++)
            {
                T attr = objects[i] as T;
                if (attr != null)
                {
                    list.Add(attr);
                    idx++;
                }
            }
            list.Capacity = idx;

            return new ReadOnlyCollection<T>(list);
        }

        internal static Type UnwrapIfTask(Type type)
        {
            if (typeof(Task).IsAssignableFrom(type))
            {
                if (type.IsGenericType)
                {
                    Type innerType = type.GetGenericArguments()[0];
                    return innerType;
                }
                else
                {
                    return typeof(void);
                }
            }
            else
            {
                return type;
            }
        }
    }
}
