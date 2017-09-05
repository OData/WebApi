// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using System.Web.OData.Properties;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// A container of property names and property values.
    /// </summary>
    /// <remarks>
    /// EntityFramework understands only member initializations in Select expressions. Also, it doesn't understand type casts for non-primitive types. So, 
    /// SelectExpandBinder has to generate strongly types expressions that involve only property access. This class represents the base class for a bunch of 
    /// generic derived types that are used in the expressions that SelectExpandBinder generates.
    /// Also, Expression.Compile() could fail with stack overflow if expression is to deep and causes to mane levels of recursion. To avoid that we are tree-like property container.
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
        /// <param name="includeAutoSelected">Specifies whether auto selected properties should be included.</param>
        /// <param name="propertyMapper">An object responsible to map the properties in this property container to the
        /// the value that will be used as the key in the dictionary we are adding properties to.</param>
        public abstract void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
            bool includeAutoSelected);

        // Expression:
        //      new NamedProperty<T> 
        //      {
        //          Name = properties[0].Key, 
        //          Value = properties[0].Value,
        //
        //          Next0 = new NamedProperty<> { ..... } 
        //          Next2 = new NamedProperty<> { ..... } 
        //      }
        public static Expression CreatePropertyContainer(IList<NamedPropertyExpression> properties)
        {
            Expression container = null;

            // build the linked list of properties.
            if (properties.Count >= 1)
            {
                NamedPropertyExpression property = properties.First();
                int count = properties.Count - 1;
                List<Expression> nextExpressions = new List<Expression>();
                int parts = 3;
                int offset = 0;
                for (int step = parts; step > 0; step--)
                {
                    int leftSize = GetLeftSize(count - offset, step);
                    nextExpressions.Add(CreatePropertyContainer(properties.Skip(1 + offset).Take(leftSize).ToList()));
                    offset += leftSize;
                }

                container = CreateNamedPropertyCreationExpression(property, nextExpressions.Where(e => e!= null).ToList());
            }

            return container;
        }

        private static int GetLeftSize(int count, int parts)
        {
            if (count % parts != 0)
            {
                return count / parts + 1;
            }
            return count / parts;
        }

        // Expression:
        // new NamedProperty<T> { Name = property.Name, Value = property.Value, Next0 = leftNext, Next2 = Next2 }.
        private static Expression CreateNamedPropertyCreationExpression(NamedPropertyExpression property, IList<Expression> expressions)
        {
            Contract.Assert(property != null);
            Contract.Assert(property.Value != null);

            Type namedPropertyType = GetNamedPropertyType(property, expressions);
            List<MemberBinding> memberBindings = new List<MemberBinding>();

            memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Name"), property.Name));

            if (property.PageSize != null || property.CountOption != null)
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Collection"), property.Value));

                if (property.PageSize != null)
                {
                    memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("PageSize"),
                        Expression.Constant(property.PageSize)));
                }

                if (property.CountOption != null && property.CountOption.Value)
                {
                    memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("TotalCount"), property.TotalCount));
                }
            }
            else
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Value"), property.Value));
            }

            for (int i = 0; i < expressions.Count; i++)
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Next" + i), expressions[i]));
            }

            if (property.NullCheck != null)
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("IsNull"), property.NullCheck));
            }

            return Expression.MemberInit(Expression.New(namedPropertyType), memberBindings);
        }

        private static List<Type> _singleExpandedPropertyTypes = new List<Type> { typeof(SingleExpandedProperty<>), typeof(SingleExpandedPropertyWithNext0<>), typeof(SingleExpandedPropertyWithNext1<>), typeof(SingleExpandedPropertyWithNext2<>) };
        private static List<Type> _collectionExpandedPropertyTypes = new List<Type> { typeof(CollectionExpandedProperty<>), typeof(CollectionExpandedPropertyWithNext0<>), typeof(CollectionExpandedPropertyWithNext1<>), typeof(CollectionExpandedPropertyWithNext2<>) };
        private static List<Type> _autoSelectedNamedPropertyTypes = new List<Type> { typeof(AutoSelectedNamedProperty<>), typeof(AutoSelectedNamedPropertyWithNext0<>), typeof(AutoSelectedNamedPropertyWithNext1<>), typeof(AutoSelectedNamedPropertyWithNext2<>) };
        private static List<Type> _namedPropertyTypes = new List<Type> { typeof(NamedProperty<>), typeof(NamedPropertyWithNext0<>), typeof(NamedPropertyWithNext1<>), typeof(NamedPropertyWithNext2<>) };

        private static Type GetNamedPropertyType(NamedPropertyExpression property, IList<Expression> expressions)
        {
            Type namedPropertyGenericType;

            if (property.NullCheck != null)
            {
                namedPropertyGenericType = _singleExpandedPropertyTypes[expressions.Count];
            }
            else if (property.PageSize != null || property.CountOption != null)
            {
                namedPropertyGenericType = _collectionExpandedPropertyTypes[expressions.Count];
            }
            else if (property.AutoSelected)
            {
                namedPropertyGenericType = _autoSelectedNamedPropertyTypes[expressions.Count];
            }
            else
            {
                namedPropertyGenericType = _namedPropertyTypes[expressions.Count];
            }

            Type elementType = (property.PageSize == null && property.CountOption == null)
                ? property.Value.Type
                : property.Value.Type.GetInnerElementType();
            return namedPropertyGenericType.MakeGenericType(elementType);
        }

        internal class NamedProperty<T> : PropertyContainer
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

            public long? TotalCount { get; set; }

            public IEnumerable<T> Collection { get; set; }

            public override object GetValue()
            {
                if (TotalCount == null)
                {
                    return new TruncatedCollection<T>(Collection, PageSize);
                }
                else
                {
                    return new TruncatedCollection<T>(Collection, PageSize, TotalCount);
                }
            }
        }

        // Entityframework requires that the two different type initializers for a given type in the same query have the same set of properties in the same order.
        // A $select=Prop1,Prop2,Prop3 where Prop1 and Prop2 are of the same type without this extra NamedPropertyWithNext type results in an select expression that looks like,
        //      c => new NamedProperty<int> { Name = "Prop1", Value = c.Prop1, Next0 = new NamedProperty<int> { Name = "Prop2", Value = c.Prop2 }, Next2 = new NamedProperty<int> { Name = "Prop3", Value = c.Prop3 } };
        // Entityframework cannot translate this expression as the first NamedProperty<int> initialization has Next and the second one doesn't. Also, Entityframework cannot 
        // create null's of NamedProperty<T>. So, you cannot generate an expression like new NamedProperty<int> { Next = null }. The exception that EF throws looks like this,
        // "The type 'NamedProperty`1[SystemInt32...]' appears in two structurally incompatible initializations within a single LINQ to Entities query. 
        // A type can be initialized in two places in the same query, but only if the same properties are set in both places and those properties are set in the same order."
        internal class NamedPropertyWithNext0<T> : NamedProperty<T>
        {
            public PropertyContainer Next0 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next0.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        internal class NamedPropertyWithNext1<T> : NamedPropertyWithNext0<T>
        {
            public PropertyContainer Next1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                if (Next1 != null)
                {
                    Next1.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                }
            }
        }

        internal class NamedPropertyWithNext2<T> : NamedPropertyWithNext1<T>
        {
            public PropertyContainer Next2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                if (Next2 != null)
                {
                    Next2.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                }
            }
        }

        private class AutoSelectedNamedPropertyWithNext0<T> : AutoSelectedNamedProperty<T>
        {
            public PropertyContainer Next0 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next0.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class AutoSelectedNamedPropertyWithNext1<T> : AutoSelectedNamedPropertyWithNext0<T>
        {
            public PropertyContainer Next1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next1.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class AutoSelectedNamedPropertyWithNext2<T> : AutoSelectedNamedPropertyWithNext1<T>
        {
            public PropertyContainer Next2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next2.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class SingleExpandedPropertyWithNext0<T> : SingleExpandedProperty<T>
        {
            public PropertyContainer Next0 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next0.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class SingleExpandedPropertyWithNext1<T> : SingleExpandedPropertyWithNext0<T>
        {
            public PropertyContainer Next1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next1.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }


        private class SingleExpandedPropertyWithNext2<T> : SingleExpandedPropertyWithNext1<T>
        {
            public PropertyContainer Next2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next2.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class CollectionExpandedPropertyWithNext0<T> : CollectionExpandedProperty<T>
        {
            public PropertyContainer Next0 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next0.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class CollectionExpandedPropertyWithNext1<T> : CollectionExpandedPropertyWithNext0<T>
        {
            public PropertyContainer Next1 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next1.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }

        private class CollectionExpandedPropertyWithNext2<T> : CollectionExpandedPropertyWithNext1<T>
        {
            public PropertyContainer Next2 { get; set; }

            public override void ToDictionaryCore(Dictionary<string, object> dictionary, IPropertyMapper propertyMapper,
                bool includeAutoSelected)
            {
                base.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
                Next2.ToDictionaryCore(dictionary, propertyMapper, includeAutoSelected);
            }
        }
    }
}
