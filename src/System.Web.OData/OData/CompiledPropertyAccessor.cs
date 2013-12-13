// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq.Expressions;
using System.Reflection;
using System.Web.Http.OData.Formatter.Deserialization;

namespace System.Web.Http.OData
{
    /// <summary>
    /// CompiledPropertyAccessor is a <see cref="PropertyAccessor{TEntityType}"/> that pre-compiles (using expression)
    /// a Getter and Setter for the PropertyInfo of TEntityType provided via the constructor.
    /// </summary>
    /// <typeparam name="TEntityType">The type on which the PropertyInfo exists</typeparam>
    internal class CompiledPropertyAccessor<TEntityType> : PropertyAccessor<TEntityType> where TEntityType : class
    {
        private bool _isCollection;
        private PropertyInfo _property;
        private Action<TEntityType, object> _setter;
        private Func<TEntityType, object> _getter;

        public CompiledPropertyAccessor(PropertyInfo property)
            : base(property)
        {
            _property = property;
            _isCollection = property.PropertyType.IsCollection();
            _getter = MakeGetter(Property);

            if (!_isCollection)
            {
                _setter = MakeSetter(property);
            }
        }

        public override object GetValue(TEntityType entity)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }
            return _getter(entity);
        }

        public override void SetValue(TEntityType entity, object value)
        {
            if (entity == null)
            {
                throw Error.ArgumentNull("entity");
            }

            if (_isCollection)
            {
                DeserializationHelpers.SetCollectionProperty(entity, _property.Name, edmPropertyType: null,
                    value: value, clearCollection: true);
            }
            else
            {
                _setter(entity, value);
            }
        }

        private static Action<TEntityType, object> MakeSetter(PropertyInfo property)
        {
            Type type = typeof(TEntityType);
            ParameterExpression entityParameter = Expression.Parameter(type);
            ParameterExpression objectParameter = Expression.Parameter(typeof(object));
            MemberExpression toProperty = Expression.Property(Expression.TypeAs(entityParameter, property.DeclaringType), property);
            UnaryExpression fromValue = Expression.Convert(objectParameter, property.PropertyType);
            BinaryExpression assignment = Expression.Assign(toProperty, fromValue);
            Expression<Action<TEntityType, object>> lambda = Expression.Lambda<Action<TEntityType, object>>(assignment, entityParameter, objectParameter);
            return lambda.Compile();
        }

        private static Func<TEntityType, object> MakeGetter(PropertyInfo property)
        {
            Type type = typeof(TEntityType);
            ParameterExpression entityParameter = Expression.Parameter(type);
            MemberExpression fromProperty = Expression.Property(Expression.TypeAs(entityParameter, property.DeclaringType), property);
            UnaryExpression convert = Expression.Convert(fromProperty, typeof(Object));
            Expression<Func<TEntityType, object>> lambda = Expression.Lambda<Func<TEntityType, object>>(convert, entityParameter);
            return lambda.Compile();
        }
    }
}
