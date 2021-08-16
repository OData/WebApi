//-----------------------------------------------------------------------------
// <copyright file="ExpandAttribute.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AspNet.OData.Query
{
    /// <summary>
    /// Represents an <see cref="Attribute"/> that can be placed on a property or a class
    /// correlate to OData's $expand query option settings.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments",
        Justification = "Don't want those argument to be retrievable")]
    public sealed class ExpandAttribute : Attribute
    {
        private readonly Dictionary<string, ExpandConfiguration> _expandConfigurations = new Dictionary<string, ExpandConfiguration>();
        private SelectExpandType _expandType;
        private SelectExpandType? _defaultExpandType;
        private int? _defaultMaxDepth;
        private int _maxDepth;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpandAttribute"/> class.
        /// </summary>
        public ExpandAttribute()
        {
            _defaultExpandType = SelectExpandType.Allowed;
            _defaultMaxDepth = ODataValidationSettings.DefaultMaxExpansionDepth;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpandAttribute"/> class
        /// with the name of allowed expand properties.
        /// </summary>
        public ExpandAttribute(params string[] properties)
        {
            foreach (var property in properties)
            {
                if (!_expandConfigurations.ContainsKey(property))
                {
                    _expandConfigurations.Add(property, new ExpandConfiguration
                    {
                        ExpandType = SelectExpandType.Allowed,
                        MaxDepth = ODataValidationSettings.DefaultMaxExpansionDepth
                    });
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ExpandConfiguration"/> of navigation properties.
        /// </summary>
        public Dictionary<string, ExpandConfiguration> ExpandConfigurations
        {
            get
            {
                return _expandConfigurations;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ExpandType"/> of navigation properties.
        /// </summary>
        public SelectExpandType ExpandType
        {
            get
            {
                return _expandType;
            }
            set
            {
                _expandType = value;
                foreach (var key in _expandConfigurations.Keys)
                {
                    _expandConfigurations[key].ExpandType = _expandType;
                }

                if (_expandConfigurations.Count == 0)
                {
                    _defaultExpandType = _expandType;
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum expand depth of navigation properties.
        /// </summary>
        public int MaxDepth
        {
            get
            {
                return _maxDepth;
            }
            set
            {
                _maxDepth = value;
                foreach (var key in _expandConfigurations.Keys)
                {
                    _expandConfigurations[key].MaxDepth = _maxDepth;
                }

                if (_expandConfigurations.Count == 0)
                {
                    _defaultMaxDepth = _maxDepth;
                }
            }
        }

        internal SelectExpandType? DefaultExpandType
        {
            get
            {
                return _defaultExpandType;
            }
            set
            {
                _defaultExpandType = value;
            }
        }

        internal int? DefaultMaxDepth
        {
            get
            {
                return _defaultMaxDepth;
            }
            set
            {
                _defaultMaxDepth = value;
            }
        }
    }
}
