// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Web.Http.OData.Query.Expressions
{
    /// <summary>
    /// A container of property names and property values.
    /// </summary>
    /// <remarks>
    /// EntityFramework understands only member initializations in Select expressions. Also, it doesn't understand type casts for non-primitive types. So, 
    /// SelectExpandBinder has to generate strongly types expressions that involve only property access. This class represents the base class for a bunch of 
    /// generic derived types that are used in the expressions that SelectExpandBinder generates.
    /// </remarks>
    internal abstract class PropertyContainer
    {
        private static readonly Type[] _containerTypes = new Type[]
        {
            typeof(PropertyContainerImpl<>), 
            typeof(PropertyContainerImpl<,>),
            typeof(PropertyContainerImpl<,,>),
            typeof(PropertyContainerImpl<,,,>),
            typeof(PropertyContainerImpl<,,,,>),
            typeof(PropertyContainerImpl<,,,,,>),
        };

        private static readonly Type _chainingContainerType = typeof(ChainingPropertyContainerImpl<,,,,,>);

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyContainer"/> class.
        /// </summary>
        protected PropertyContainer()
        {
        }

        /// <summary>
        /// Builds the dictionary of properties in this container keyed by the property name.
        /// </summary>
        /// <returns>The dictionary of properties in this container keyed by the property name.</returns>
        public Dictionary<string, object> ToDictionary(bool includeAutoSelected = true)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            ToDictionaryCore(result, includeAutoSelected);
            return result;
        }

        /// <summary>
        /// Adds the properties in this container to the give dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to add the properties to.</param>
        /// <param name="includeAutoSelected">Specifies whether auto selected properties should be included.</param>
        public abstract void ToDictionaryCore(Dictionary<string, object> dictionary, bool includeAutoSelected);

        // Expression:
        //      new PropertyContainer<...> 
        //      {
        //          Property1 = new NamedProperty { Name = properties[0].Key, Value = properties[0].Value }, 
        //          Property2 = new NamedProperty { Name = properties[1].Key, Value = properties[2].Value }
        //          ... 
        //          Next = new PropertyContainer<...> { ..... } 
        //      }
        public static Expression CreatePropertyContainer(IList<NamedPropertyExpression> properties, int start = 0)
        {
            Type _containerGenericType;

            int propertiesInNextContainerStart = -1;
            int propertiesInThisContainerCount = properties.Count - start;

            if (propertiesInThisContainerCount > _containerTypes.Length)
            {
                propertiesInThisContainerCount = _containerTypes.Length;
                propertiesInNextContainerStart = start + _containerTypes.Length;
                _containerGenericType = _chainingContainerType;
            }
            else
            {
                _containerGenericType = _containerTypes[propertiesInThisContainerCount - 1];
            }

            Type[] containerPropertyTypes = new Type[propertiesInThisContainerCount];
            for (int i = 0; i < propertiesInThisContainerCount; i++)
            {
                containerPropertyTypes[i] = properties[start + i].Value.Type;
            }
            Type containerType = _containerGenericType.MakeGenericType(containerPropertyTypes);

            List<MemberBinding> memberBindings = new List<MemberBinding>();
            for (int i = 0; i < propertiesInThisContainerCount; i++)
            {
                Expression propertyNameExpression = properties[start + i].Name;
                Expression propertyValueExpression = properties[start + i].Value;
                bool isAutoSelected = properties[start + i].AutoSelected;

                PropertyInfo namedProperty = GetContainerProperty(containerType, memberBindings.Count);
                Expression namedPropertyValue = CreateNamedPropertyCreationExpression(propertyNameExpression, propertyValueExpression, isAutoSelected);

                memberBindings.Add(Expression.Bind(namedProperty, namedPropertyValue));
            }

            if (propertiesInNextContainerStart != -1)
            {
                Expression nextContainerInitializer = CreatePropertyContainer(properties, propertiesInNextContainerStart);
                memberBindings.Add(Expression.Bind(containerType.GetProperty("Next"), nextContainerInitializer));
            }

            return Expression.MemberInit(Expression.New(containerType), memberBindings);
        }

        private static Expression CreateNamedPropertyCreationExpression(Expression name, Expression value, bool isAutoSelected)
        {
            Contract.Assert(name != null);
            Contract.Assert(value != null);

            Type namedPropertyType = isAutoSelected ? typeof(AutoSelectedNamedProperty<>) : typeof(NamedProperty<>);
            namedPropertyType = namedPropertyType.MakeGenericType(value.Type);

            Expression namedPropertyCreationExpression =
                Expression.MemberInit(
                    Expression.New(namedPropertyType),
                    Expression.Bind(namedPropertyType.GetProperty("Name"), name),
                    Expression.Bind(namedPropertyType.GetProperty("Value"), value));

            return namedPropertyCreationExpression;
        }

        private static PropertyInfo GetContainerProperty(Type containerType, int index)
        {
            return containerType.GetProperty("Value" + (index + 1));
        }

        private class NamedProperty<T>
        {
            public string Name { get; set; }

            public T Value { get; set; }

            public bool AutoSelected { get; set; }

            internal void AddToDictionary(Dictionary<string, object> dictionary, bool includeAutoSelected)
            {
                Contract.Assert(dictionary != null);

                if (Name != null && (includeAutoSelected || !AutoSelected))
                {
                    dictionary.Add(Name, Value);
                }
            }
        }

        private class AutoSelectedNamedProperty<T> : NamedProperty<T>
        {
            public AutoSelectedNamedProperty()
            {
                AutoSelected = true;
            }
        }

        private class PropertyContainerImpl<TProp1>
        : PropertyContainer
        {
            public NamedProperty<TProp1> Value1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, bool includeAutoSelected)
            {
                Value1.AddToDictionary(dictionary, includeAutoSelected);
            }
        }

        private class PropertyContainerImpl<TProp1, TProp2>
            : PropertyContainerImpl<TProp1>
        {
            public NamedProperty<TProp2> Value2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, includeAutoSelected);
                Value2.AddToDictionary(dictionary, includeAutoSelected);
            }
        }

        private class PropertyContainerImpl<TProp1, TProp2, TProp3>
            : PropertyContainerImpl<TProp1, TProp2>
        {
            public NamedProperty<TProp3> Value3 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, includeAutoSelected);
                Value3.AddToDictionary(dictionary, includeAutoSelected);
            }
        }

        private class PropertyContainerImpl<TProp1, TProp2, TProp3, TProp4>
            : PropertyContainerImpl<TProp1, TProp2, TProp3>
        {
            public NamedProperty<TProp4> Value4 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, includeAutoSelected);
                Value4.AddToDictionary(dictionary, includeAutoSelected);
            }
        }

        private class PropertyContainerImpl<TProp1, TProp2, TProp3, TProp4, TProp5>
            : PropertyContainerImpl<TProp1, TProp2, TProp3, TProp4>
        {
            public NamedProperty<TProp5> Value5 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, includeAutoSelected);
                Value5.AddToDictionary(dictionary, includeAutoSelected);
            }
        }

        private class PropertyContainerImpl<TProp1, TProp2, TProp3, TProp4, TProp5, TProp6>
            : PropertyContainerImpl<TProp1, TProp2, TProp3, TProp4, TProp5>
        {
            public NamedProperty<TProp6> Value6 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, includeAutoSelected);
                Value6.AddToDictionary(dictionary, includeAutoSelected);
            }
        }

        private class ChainingPropertyContainerImpl<TProp1, TProp2, TProp3, TProp4, TProp5, TProp6>
            : PropertyContainerImpl<TProp1, TProp2, TProp3, TProp4, TProp5, TProp6>
        {
            public PropertyContainer Next { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, includeAutoSelected);
                Next.ToDictionaryCore(dictionary, includeAutoSelected);
            }
        }
    }
}
