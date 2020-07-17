// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.OData.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData;

namespace Microsoft.AspNet.OData.Extensions
{
    internal static class ODataConfigurationExtensions
    {
        public static void SetDefaultQuerySettings(this IServiceProvider serviceProvider, DefaultQuerySettings defaultQuerySettings)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            if (defaultQuerySettings == null)
            {
                throw Error.ArgumentNull(nameof(defaultQuerySettings));
            }

            DefaultQuerySettings querySettings = serviceProvider.GetRequiredService<DefaultQuerySettings>();
            if (querySettings == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(DefaultQuerySettings));
            }

            querySettings.CopySettings(defaultQuerySettings);
            
            if (!defaultQuerySettings.MaxTop.HasValue || defaultQuerySettings.MaxTop > 0)
            {
                ModelBoundQuerySettings.DefaultModelBoundQuerySettings.MaxTop = defaultQuerySettings.MaxTop;
            }
        }

        public static DefaultQuerySettings GetDefaultQuerySettings(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings querySettings = serviceProvider.GetRequiredService<DefaultQuerySettings>();
            if (querySettings == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(DefaultQuerySettings));
            }

            return querySettings;
        }

        public static void MaxTop(this IServiceProvider serviceProvider, int? maxTopValue)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.MaxTop = maxTopValue;
            if (!maxTopValue.HasValue || maxTopValue > 0)
            {
                ModelBoundQuerySettings.DefaultModelBoundQuerySettings.MaxTop = maxTopValue;
            }
        }

        public static void Expand(this IServiceProvider serviceProvider, QueryOptionSetting setting)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableExpand = setting == QueryOptionSetting.Allowed;
        }

        public static void Expand(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableExpand = true;
        }

        public static void Select(this IServiceProvider serviceProvider, QueryOptionSetting setting)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableSelect = setting == QueryOptionSetting.Allowed;
        }

        public static void Select(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableSelect = true;
        }

        public static void Filter(this IServiceProvider serviceProvider, QueryOptionSetting setting)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableFilter = setting == QueryOptionSetting.Allowed;
        }

        public static void Filter(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableFilter = true;
        }

        public static void OrderBy(this IServiceProvider serviceProvider, QueryOptionSetting setting)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableOrderBy = setting == QueryOptionSetting.Allowed;
        }

        public static void OrderBy(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableOrderBy = true;
        }

        public static void Count(this IServiceProvider serviceProvider, QueryOptionSetting setting)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableCount = setting == QueryOptionSetting.Allowed;
        }

        public static void Count(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableCount = true;
        }

        public static void SkipToken(this IServiceProvider serviceProvider, QueryOptionSetting setting)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableSkipToken = setting == QueryOptionSetting.Allowed;
        }

        public static void SkipToken(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            DefaultQuerySettings defaultQuerySettings = serviceProvider.GetDefaultQuerySettings();
            defaultQuerySettings.EnableSkipToken = true;
        }

        public static void SetDefaultODataOptions(this IServiceProvider serviceProvider, ODataOptions defaultOptions)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            if (defaultOptions == null)
            {
                throw Error.ArgumentNull(nameof(defaultOptions));
            }

            ODataOptions options = serviceProvider.GetRequiredService<ODataOptions>();
            if (options == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(ODataOptions));
            }

            options.CompatibilityOptions = defaultOptions.CompatibilityOptions;
            options.EnableContinueOnErrorHeader = defaultOptions.EnableContinueOnErrorHeader;
            options.NullDynamicPropertyIsEnabled = defaultOptions.NullDynamicPropertyIsEnabled;
            options.UrlKeyDelimiter = defaultOptions.UrlKeyDelimiter;
            options.EnableCaseInsensitiveModelBinding = defaultOptions.EnableCaseInsensitiveModelBinding;
        }

        public static ODataOptions GetDefaultODataOptions(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw Error.ArgumentNull(nameof(serviceProvider));
            }

            ODataOptions options = serviceProvider.GetRequiredService<ODataOptions>();
            if (options == null)
            {
                throw Error.InvalidOperation(SRResources.MissingODataServices, nameof(ODataOptions));
            }

            return options;
        }
    }
}
