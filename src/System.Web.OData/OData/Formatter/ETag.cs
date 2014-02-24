// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Http;
using System.Web.OData.Properties;
using System.Web.OData.Query.Expressions;

namespace System.Web.OData.Formatter
{
    /// <summary>
    /// The ETag parsed from request.
    /// </summary>
    public class ETag : DynamicObject
    {
        private IDictionary<string, object> _concurrencyProperties = new Dictionary<string, object>();

        /// <summary>
        /// Create an instance of <see cref="ETag"/>.
        /// </summary>
        public ETag()
        {
            IsWellFormed = true;
        }

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        public object this[string key]
        {
            get
            {
                if (!IsWellFormed)
                {
                    throw Error.InvalidOperation(SRResources.ETagNotWellFormed);
                }
                return ConcurrencyProperties[key];
            }
            set
            {
                ConcurrencyProperties[key] = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the ETag is well-formed.
        /// </summary>
        public bool IsWellFormed { get; set; }

        /// <summary>
        /// Gets or sets an entity type of the ETag.
        /// </summary>
        public Type EntityType { get; set; }

        internal bool IsIfNoneMatch { get; set; }

        internal IDictionary<string, object> ConcurrencyProperties
        {
            get
            {
                return _concurrencyProperties;
            }
            set
            {
                _concurrencyProperties = value;
            }
        }

        /// <summary>
        /// Gets a property value from the ETag.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            }

            if (!IsWellFormed)
            {
                throw Error.InvalidOperation(SRResources.ETagNotWellFormed);
            } 
            
            string name = binder.Name;
            return ConcurrencyProperties.TryGetValue(name, out result);
        }

        /// <summary>
        /// Sets a property value to ETag.
        /// </summary>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (binder == null)
            {
                throw Error.ArgumentNull("binder");
            } 
            
            ConcurrencyProperties[binder.Name] = value;
            return true;
        }

        /// <summary>
        /// Apply the ETag to the given IQueryable.
        /// </summary>
        /// <param name="query">The original <see cref="IQueryable"/>.</param>
        /// <returns>The new <see cref="IQueryable"/> after the ETag has been applied to.</returns>
        public virtual IQueryable ApplyTo(IQueryable query)
        {
            Type type = EntityType;
            ParameterExpression param = Expression.Parameter(type);
            Expression where = null;
            foreach (KeyValuePair<string, object> item in ConcurrencyProperties)
            {
                MemberExpression name = Expression.Property(param, item.Key);
                object itemValue = item.Value;
                Expression value = itemValue != null
                    ? LinqParameterContainer.Parameterize(itemValue.GetType(), itemValue)
                    : Expression.Constant(value: null);
                BinaryExpression equal = Expression.Equal(name, value);
                where = where == null ? equal : Expression.AndAlso(where, equal);
            }

            if (where == null)
            {
                return query;
            }

            if (IsIfNoneMatch)
            {
                where = Expression.Not(where);
            }

            Expression whereLambda = Expression.Lambda(where, param);
            return ExpressionHelpers.Where(query, whereLambda, type);
        }
    }
}
