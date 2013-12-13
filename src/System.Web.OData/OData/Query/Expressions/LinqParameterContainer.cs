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

        // the value of the constant.
        public abstract object Property { get; }

        public static Expression Parameterize(Type type, object value)
        {
            // () => new LinqParameterContainer(constant).Property
            // instead of returning a constant expression node, wrap that constant in a class the way compiler 
            // does a closure, so that EF can parameterize the constant (resulting in better performance due to expression translation caching).
            LinqParameterContainer containedValue = LinqParameterContainer.Create(type, value);
            return Expression.Property(Expression.Constant(containedValue), "TypedProperty");
        }

        private static LinqParameterContainer Create(Type type, object value)
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
            public TypedLinqParameterContainer(T value)
            {
                TypedProperty = value;
            }

            public T TypedProperty { get; set; }

            public override object Property
            {
                get { return TypedProperty; }
            }
        }
    }
}
