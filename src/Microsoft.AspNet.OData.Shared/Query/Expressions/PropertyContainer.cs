// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.AspNet.OData.Common;

namespace Microsoft.AspNet.OData.Query.Expressions
{
    /// <summary>
    /// A container of property names and property values.
    /// </summary>
    /// <remarks>
    /// EntityFramework understands only member initializations in Select expressions. Also, it doesn't understand type casts for non-primitive types. So, 
    /// SelectExpandBinder has to generate strongly types expressions that involve only property access. This class represents the base class for a bunch of 
    /// generic derived types that are used in the expressions that SelectExpandBinder generates.
    /// Also, Expression.Compile() could fail with stack overflow if expression is to deep and causes too many levels of recursion. To avoid that we are b-tree property container.
    /// </remarks>
    internal abstract partial class PropertyContainer
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
        //          Next1 = new NamedProperty<> { ..... },
        //          ...
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
                int parts = SingleExpandedPropertyTypes.Count - 1;
                int offset = 0;
                for (int step = parts; step > 0; step--)
                {
                    int leftSize = GetLeftSize(count - offset, step);
                    nextExpressions.Add(CreatePropertyContainer(properties.Skip(1 + offset).Take(leftSize).ToList()));
                    offset += leftSize;
                }

                container = CreateNamedPropertyCreationExpression(property, nextExpressions.Where(e => e != null).ToList());
            }

            return container;
        }

        private static int GetLeftSize(int count, int parts)
        {
            if (count % parts != 0)
            {
                return (count / parts) + 1;
            }
            return count / parts;
        }

        // Expression:
        // new NamedProperty<T> { Name = property.Name, Value = property.Value, Next0 = next0, Next1 = next1, .... }.
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
                    memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("TotalCount"), ExpressionHelpers.ToNullable(property.TotalCount)));
                }
            }
            else
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Value"), property.Value));
            }

            for (int i = 0; i < expressions.Count; i++)
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("Next" + i.ToString(CultureInfo.CurrentCulture)), expressions[i]));
            }

            if (property.NullCheck != null)
            {
                memberBindings.Add(Expression.Bind(namedPropertyType.GetProperty("IsNull"), property.NullCheck));
            }

            return Expression.MemberInit(Expression.New(namedPropertyType), memberBindings);
        }

        private static Type GetNamedPropertyType(NamedPropertyExpression property, IList<Expression> expressions)
        {
            Type namedPropertyGenericType;

            if (property.NullCheck != null)
            {
                namedPropertyGenericType = SingleExpandedPropertyTypes[expressions.Count];
            }
            else if (property.PageSize != null || property.CountOption != null)
            {
                namedPropertyGenericType = CollectionExpandedPropertyTypes[expressions.Count];
            }
            else if (property.AutoSelected)
            {
                namedPropertyGenericType = AutoSelectedNamedPropertyTypes[expressions.Count];
            }
            else
            {
                namedPropertyGenericType = NamedPropertyTypes[expressions.Count];
            }

            Type elementType = (property.PageSize == null && property.CountOption == null)
                ? property.Value.Type
                : TypeHelper.GetInnerElementType(property.Value.Type);
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
                if (Collection == null)
                {
                    return null;
                }

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
    }
}
