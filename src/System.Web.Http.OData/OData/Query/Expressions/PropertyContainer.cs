// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Web.Http.OData.Properties;

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
        public Dictionary<string, object> ToDictionary(IPropertyMapper propertyMapper, bool includeAutoSelected = true)
        {
            Contract.Assert(propertyMapper != null);
            Dictionary<string, object> result = new Dictionary<string, object>();
            ToDictionaryCore(result, propertyMapper, includeAutoSelected);
            return result;
        }

        /// <summary>
        /// Adds the properties in this container to the given dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to add the properties to.</param>
        /// <param name="propertyMapper">An object responsible to map the properties in this property container to the
        /// the value that will be used as the key in the dictionary we are adding properties to.</param>
        /// <param name="includeAutoSelected">Specifies whether auto selected properties should be included.</param>
        public abstract void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
            bool includeAutoSelected);

        // Expression:
        //      new NamedProperty<T> 
        //      {
        //          Name = properties[0].Key, 
        //          Value = properties[0].Value,
        //
        //          Next = new NamedProperty<> { ..... } 
        //      }
        public static Expression CreatePropertyContainer(IList<NamedPropertyExpression> properties)
        {
            Expression container = null;

            // build the linked list of properties.
            foreach (NamedPropertyExpression property in properties)
            {
                container = CreateNamedPropertyCreationExpression(property, container);
            }

            return container;
        }

        // Expression:
        // new NamedProperty<T> { Name = property.Name, Value = property.Value, Next = next }.
        private static Expression CreateNamedPropertyCreationExpression(NamedPropertyExpression property, Expression next)
        {
            Contract.Assert(property != null);
            Contract.Assert(property.Value != null);

            Type namedPropertyType = GetNamedPropertyType(property, next);
            List<MemberBinding> memberBindings = new List<MemberBinding>();

            memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Name"), property.Name));

            if (property.PageSize == null)
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Value"), property.Value));
            }
            else
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Collection"), property.Value));
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("PageSize"), Expression.Constant(property.PageSize)));
            }

            if (next != null)
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Next"), next));
            }
            if (property.NullCheck != null)
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("IsNull"), property.NullCheck));
            }

            return Expression.MemberInit(Expression.New(namedPropertyType), memberBindings);
        }

        private static Type GetNamedPropertyType(NamedPropertyExpression property, Expression next)
        {
            Type namedPropertyGenericType;

            if (next == null)
            {
                if (property.NullCheck != null)
                {
                    namedPropertyGenericType = typeof(SingleExpandedProperty<>);
                }
                else if (property.PageSize != null)
                {
                    namedPropertyGenericType = typeof(CollectionExpandedProperty<>);
                }
                else if (property.AutoSelected)
                {
                    namedPropertyGenericType = typeof(AutoSelectedNamedProperty<>);
                }
                else
                {
                    namedPropertyGenericType = typeof(NamedProperty<>);
                }
            }
            else
            {
                if (property.NullCheck != null)
                {
                    namedPropertyGenericType = typeof(SingleExpandedPropertyWithNext<>);
                }
                else if (property.PageSize != null)
                {
                    namedPropertyGenericType = typeof(CollectionExpandedPropertyWithNext<>);
                }
                else if (property.AutoSelected)
                {
                    namedPropertyGenericType = typeof(AutoSelectedNamedPropertyWithNext<>);
                }
                else
                {
                    namedPropertyGenericType = typeof(NamedPropertyWithNext<>);
                }
            }

            Type elementType = property.PageSize == null ? property.Value.Type : property.Value.Type.GetInnerElementType();
            return namedPropertyGenericType.MakeGenericType(elementType);
        }

        private class NamedProperty<T> : PropertyContainer
        {
            public string Name { get; set; }

            public T Value { get; set; }

            public bool AutoSelected { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                Contract.Assert(dictionary != null);

                if (Name != null && (includeAutoSelected || !AutoSelected))
                {
                    string mappedName = propertyMapper.MapProperty(Name);
                    if (String.IsNullOrEmpty(mappedName))
                    {
                        throw Error.InvalidOperation(SRResources.InvalidPropertyMapping, Name);
                    }

                    dictionary.Add(mappedName, GetValue());
                }
            }

            public virtual object GetValue()
            {
                return Value;
            }
        }

        private class AutoSelectedNamedProperty<T> : NamedProperty<T>
        {
            public AutoSelectedNamedProperty()
            {
                AutoSelected = true;
            }
        }

        private class SingleExpandedProperty<T> : NamedProperty<T>
        {
            public bool IsNull { get; set; }

            public override object GetValue()
            {
                return IsNull ? (object)null : Value;
            }
        }

        private class CollectionExpandedProperty<T> : NamedProperty<T>
        {
            public int PageSize { get; set; }

            public IEnumerable<T> Collection { get; set; }

            public override object GetValue()
            {
                return new TruncatedCollection<T>(Collection, PageSize);
            }
        }

        // Entityframework requires that the two different type initializers for a given type in the same query have the same set of properties in the same order.
        // A $select=Prop1,Prop2 where Prop1 and Prop2 are of the same type without this extra NamedPropertyWithNext type results in an select expression that looks like,
        //      c => new NamedProperty<int> { Name = "Prop1", Value = c.Prop1, Next = new NamedProperty<int> { Name = "Prop2", Value = c.Prop2 } };
        // Entityframework cannot translate this expression as the first NamedProperty<int> initialization has Next and the second one doesn't. Also, Entityframework cannot 
        // create null's of NamedProperty<T>. So, you cannot generate an expression like new NamedProperty<int> { Next = null }. The exception that EF throws looks like this,
        // "The type 'NamedProperty`1[SystemInt32...]' appears in two structurally incompatible initializations within a single LINQ to Entities query. 
        // A type can be initialized in two places in the same query, but only if the same properties are set in both places and those properties are set in the same order."
        private class NamedPropertyWithNext<T> : NamedProperty<T>
        {
            public PropertyContainer Next { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class AutoSelectedNamedPropertyWithNext<T> : AutoSelectedNamedProperty<T>
        {
            public PropertyContainer Next { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class SingleExpandedPropertyWithNext<T> : SingleExpandedProperty<T>
        {
            public PropertyContainer Next { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class CollectionExpandedPropertyWithNext<T> : CollectionExpandedProperty<T>
        {
            public PropertyContainer Next { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
    }
}
