//-----------------------------------------------------------------------------
// <copyright file="SelectAttribute.cs" company=".NET Foundation">
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
    /// Represents an <see cref="Attribute"/> that can be placed on a property or a class
    /// correlate to OData's $select query option settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments",
        Justification = "Don't want those argument to be retrievable")]
    public sealed class SelectAttribute : Attribute
    {
        private readonly Dictionary<string, SelectExpandType> _selectConfigurations = new Dictionary<string, SelectExpandType>();
        private SelectExpandType _selectType;
        private SelectExpandType? _defaultSelectType;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAttribute"/> class.
        /// </summary>
        public SelectAttribute()
        {
            _defaultSelectType = SelectExpandType.Allowed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectAttribute"/> class
        /// with the name of allowed select properties.
        /// </summary>
        public SelectAttribute(params string[] properties)
        {
            foreach (var property in properties)
            {
                if (!_selectConfigurations.ContainsKey(property))
                {
                    _selectConfigurations.Add(property, SelectExpandType.Allowed);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SelectExpandType"/> of properties.
        /// </summary>
        public Dictionary<string, SelectExpandType> SelectConfigurations
        {
            get
            {
                return _selectConfigurations;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="SelectExpandType"/> of properties.
        /// </summary>
        public SelectExpandType SelectType
        {
            get
            {
                return _selectType;
            }
            set
            {
                _selectType = value;
                List<string> keys = _selectConfigurations.Keys.ToList();
                foreach (var property in keys)
                {
                    _selectConfigurations[property] = _selectType;
                }

                if (_selectConfigurations.Count == 0)
                {
                    _defaultSelectType = _selectType;
                }
            }
        }

        internal SelectExpandType? DefaultSelectType
        {
            get
            {
                return _defaultSelectType;
            }
            set
            {
                _defaultSelectType = value;
            }
        }
    }
}
