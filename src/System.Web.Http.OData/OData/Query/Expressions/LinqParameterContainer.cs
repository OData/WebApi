// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Http.OData.Query.Expressions
{
    // wraps a constant value so that EntityFramework paramterizes the constant.
    internal abstract class LinqParameterContainer
    {
        private static ConcurrentDictionary<Type, Func<object, LinqParameterContainer>> _ctors = new ConcurrentDictionary<Type, Func<object, LinqParameterContainer>>();

        // the propertyinfo of the property that holds the wrapped constant.
        public abstract PropertyInfo PropertyInfo { get; }

        // the value of the constant.
        public abstract object Property { get; }

        public static LinqParameterContainer Create(Type type, object value)
        {
            return _ctors.GetOrAdd(type, t =>
                {
                    MethodInfo createMethod = typeof(LinqParameterContainer).GetMethod("CreateInternal").MakeGenericMethod(t);
                    ParameterExpression valueParameter = Expression.Parameter(typeof(object));
                    return
                        Expression.Lambda<Func<object, LinqParameterContainer>>(
                            Expression.Call(
                                createMethod,
                                Expression.Convert(valueParameter, t)),
                            valueParameter)
                        .Compile();
                })(value);
        }

        // invoked dynamically at runtime.
        public static LinqParameterContainer CreateInternal<T>(T value)
        {
            return new TypedLinqParameterContainer<T>(value);
        }

        // having a strongly typed property avoids the a cast in the property access expression that would be 
        // generated for this constant.
        internal class TypedLinqParameterContainer<T> : LinqParameterContainer
        {
            private static PropertyInfo _propertyInfo = typeof(TypedLinqParameterContainer<T>).GetProperty("TypedProperty");

            public TypedLinqParameterContainer(T value)
            {
                TypedProperty = value;
            }

            public T TypedProperty { get; set; }

            public override object Property
            {
                get { return TypedProperty; }
            }

            public override PropertyInfo PropertyInfo
            {
                get { return _propertyInfo; }
            }
        }
    }
}
