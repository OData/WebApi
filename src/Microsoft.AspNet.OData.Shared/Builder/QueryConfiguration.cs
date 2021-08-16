//-----------------------------------------------------------------------------
// <copyright file="QueryConfiguration.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using Microsoft.AspNet.OData.Query;

namespace Microsoft.AspNet.OData.Builder
{
    /// <summary>
    /// Query configuration which contains <see cref="ModelBoundQuerySettings"/>.
    /// </summary>
    public class QueryConfiguration
    {
        private ModelBoundQuerySettings _querySettings;

        /// <summary>
        /// Gets or sets the <see cref="ModelBoundQuerySettings"/>.
        /// </summary>
        public ModelBoundQuerySettings ModelBoundQuerySettings
        {
            get
            {
                return _querySettings;
            }
            set
            {
                _querySettings = value;
            }
        }

        /// <summary>
        /// Sets the Countable in <see cref="ModelBoundQuerySettings"/>.
        /// </summary>
        public virtual void SetCount(bool enableCount)
        {
            GetModelBoundQuerySettingsOrDefault().Countable = enableCount;
        }

        /// <summary>
        /// Sets the MaxTop in <see cref="ModelBoundQuerySettings"/>.
        /// </summary>
        public virtual void SetMaxTop(int? maxTop)
        {
            GetModelBoundQuerySettingsOrDefault().MaxTop = maxTop;
        }

        /// <summary>
        /// Sets the PageSize in <see cref="ModelBoundQuerySettings"/>.
        /// </summary>
        public virtual void SetPageSize(int? pageSize)
        {
            GetModelBoundQuerySettingsOrDefault().PageSize = pageSize;
        }

        /// <summary>
        /// Sets the ExpandConfigurations in <see cref="ModelBoundQuerySettings"/>.
        /// </summary>
        public virtual void SetExpand(IEnumerable<string> properties, int? maxDepth, SelectExpandType expandType)
        {
            GetModelBoundQuerySettingsOrDefault();
            if (properties == null)
            {
                ModelBoundQuerySettings.DefaultExpandType = expandType;
                ModelBoundQuerySettings.DefaultMaxDepth = maxDepth ?? ODataValidationSettings.DefaultMaxExpansionDepth;
            }
            else
            {
                foreach (var property in properties)
                {
                    ModelBoundQuerySettings.ExpandConfigurations[property] = new ExpandConfiguration
                    {
                        ExpandType = expandType,
                        MaxDepth = maxDepth ?? ODataValidationSettings.DefaultMaxExpansionDepth
                    };
                }
            }
        }

        /// <summary>
        /// Sets the SelectConfigurations in <see cref="ModelBoundQuerySettings"/>.
        /// </summary>
        public virtual void SetSelect(IEnumerable<string> properties, SelectExpandType selectType)
        {
            GetModelBoundQuerySettingsOrDefault();
            if (properties == null)
            {
                ModelBoundQuerySettings.DefaultSelectType = selectType;
            }
            else
            {
                foreach (var property in properties)
                {
                    ModelBoundQuerySettings.SelectConfigurations[property] = selectType;
                }
            }
        }

        /// <summary>
        /// Sets the OrderByConfigurations in <see cref="ModelBoundQuerySettings"/>.
        /// </summary>
        public virtual void SetOrderBy(IEnumerable<string> properties, bool enableOrderBy)
        {
            GetModelBoundQuerySettingsOrDefault();
            if (properties == null)
            {
                ModelBoundQuerySettings.DefaultEnableOrderBy = enableOrderBy;
            }
            else
            {
                foreach (var property in properties)
                {
                    ModelBoundQuerySettings.OrderByConfigurations[property] = enableOrderBy;
                }
            }
        }

        /// <summary>
        /// Sets the FilterConfigurations in <see cref="ModelBoundQuerySettings"/>.
        /// </summary>
        public virtual void SetFilter(IEnumerable<string> properties, bool enableFilter)
        {
            GetModelBoundQuerySettingsOrDefault();
            if (properties == null)
            {
                ModelBoundQuerySettings.DefaultEnableFilter = enableFilter;
            }
            else
            {
                foreach (var property in properties)
                {
                    ModelBoundQuerySettings.FilterConfigurations[property] = enableFilter;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="ModelBoundQuerySettings"/> or create it depends on the default settings.
        /// </summary>
        internal ModelBoundQuerySettings GetModelBoundQuerySettingsOrDefault()
        {
            if (_querySettings == null)
            {
                _querySettings = new ModelBoundQuerySettings(ModelBoundQuerySettings.DefaultModelBoundQuerySettings);
            }

            return _querySettings;
        }
    }
}
