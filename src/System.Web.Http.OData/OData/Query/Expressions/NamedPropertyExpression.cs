// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.Contracts;
using System.Linq.Expressions;

namespace System.Web.Http.OData.Query.Expressions
{
    /// <summary>
    /// Represents a container that captures a named property that is a part of the select expand query.
    /// </summary>
    internal class NamedPropertyExpression
    {
        public NamedPropertyExpression(Expression name, Expression value, bool autoSelected = false)
        {
            Contract.Assert(name != null);
            Contract.Assert(value != null);

            Name = name;
            Value = value;
            AutoSelected = autoSelected;
        }

        public Expression Name { get; private set; }

        public Expression Value { get; private set; }

        public bool AutoSelected { get; private set; }
    }
}
