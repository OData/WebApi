// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace System.Web.OData.Query.Expressions
{
    /// <summary>
    /// Represents a container that captures a named property that is a part of the select expand query.
    /// </summary>
    internal class NamedPropertyExpression
    {
        public NamedPropertyExpression(Expression name, Expression value)
        {
            Contract.Assert(name != null);
            Contract.Assert(value != null);

            Name = name;
            Value = value;
        }

        public Expression Name { get; private set; }

        public Expression Value { get; private set; }

        // Checks whether this property is null or not. This is required for expanded navigation properties that are null as entityframework cannot
        // create null's of type SelectExpandWrapper<ExpandedProperty> i.e. an expression like 
        //       => new NamedProperty<Customer> { Value = order.Customer == null : null : new SelectExpandWrapper<Customer> { .... } } 
        // cannot be translated by EF. So, we generate the following expression instead,
        //       => new ExpandProperty<Customer> { Value = new SelectExpandWrapper<Customer> { .... }, IsNull = nullCheck }
        // and use Value only if IsNull is false.
        public Expression NullCheck { get; set; }

        public int? PageSize { get; set; }

        public bool AutoSelected { get; set; }
    }
}
