//-----------------------------------------------------------------------------
// <copyright file="OrderByAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a class or property
    /// correlate to OData's $orderby query option settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments",
        Justification = "Don't want those argument to be retrievable")]
    public sealed class OrderByAttribute : Attribute
    {
        private bool? _defaultEnableOrderBy;
        private bool _disable;
        private readonly Dictionary<string, bool> _orderByConfigurations = new Dictionary<string, bool>();
        
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByAttribute"/> class.
        /// </summary>
        public OrderByAttribute()
        {
            _defaultEnableOrderBy = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByAttribute"/> class
        /// with the name of allowed $orderby properties.
        /// </summary>
        public OrderByAttribute(params string[] properties)
        {
            foreach (var property in properties)
            {
                if (!_orderByConfigurations.ContainsKey(property))
                {
                    _orderByConfigurations.Add(property, true);
                }
            }
        }

        /// <summary>
        /// Gets or sets the $orderby configuration of properties.
        /// </summary>
        public Dictionary<string, bool> OrderByConfigurations
        {
            get
            {
                return _orderByConfigurations;
            }
        }

        /// <summary>
        /// Represents whether the $orderby can be applied on those properties.
        /// </summary>
        public bool Disabled 
        {
            get
            {
                return _disable;
            }
            set
            {
                _disable = value;   
                List<string> keys = _orderByConfigurations.Keys.ToList();
                foreach (var property in keys)
                {
                    _orderByConfigurations[property] = !_disable;
                }

                if (_orderByConfigurations.Count == 0)
                {
                    _defaultEnableOrderBy = !_disable;
                }
            }
        }

        internal bool? DefaultEnableOrderBy
        {
            get
            {
                return _defaultEnableOrderBy;
            }
            set
            {
                _defaultEnableOrderBy = value;
            }
        }
    }
}
