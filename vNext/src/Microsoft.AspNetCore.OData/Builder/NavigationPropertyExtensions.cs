// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNetCore.OData.Builder
{
    internal static class NavigationPropertyExtensions
    {
        public static void FindAllNavigationProperties(this ODataModelBuilder builder,
            StructuralTypeConfiguration configuration,
            IList<Tuple<StructuralTypeConfiguration, IList<object>, NavigationPropertyConfiguration>> navigations,
            Stack<object> path)
        {
            if (builder == null)
            {
                throw Error.ArgumentNull("builder");
            }

            if (configuration == null)
            {
                throw Error.ArgumentNull("configuration");
            }

            if (navigations == null)
            {
                throw Error.ArgumentNull("navigations");
            }

            if (path == null)
            {
                throw Error.ArgumentNull("path");
            }

            IEnumerable<StructuralTypeConfiguration> thisAndBaseTypes = configuration.ThisAndBaseTypes();
            foreach (var config in thisAndBaseTypes)
            {
                builder.FindNavigationProperties(config, navigations, path);
            }

            IEnumerable<StructuralTypeConfiguration> derivedTypes = builder.DerivedTypes(configuration);
            foreach (var config in derivedTypes)
            {
                path.Push(config.ClrType);

                builder.FindNavigationProperties(config, navigations, path);

                path.Pop();
            }
        }

        private static void FindNavigationProperties(this ODataModelBuilder builder, StructuralTypeConfiguration configuration,
            IList<Tuple<StructuralTypeConfiguration, IList<object>, NavigationPropertyConfiguration>> navs,
            Stack<object> path)
        {
            Contract.Assert(builder != null);
            Contract.Assert(configuration != null);
            Contract.Assert(navs != null);
            Contract.Assert(path != null);

            foreach (var property in configuration.Properties)
            {
                path.Push(property.PropertyInfo);

                NavigationPropertyConfiguration nav = property as NavigationPropertyConfiguration;
                ComplexPropertyConfiguration complex = property as ComplexPropertyConfiguration;
                CollectionPropertyConfiguration collection = property as CollectionPropertyConfiguration;

                if (nav != null)
                {
                    // how about the containment?
                    IList<object> bindingPath = path.Reverse().ToList();

                    navs.Add(
                        new Tuple<StructuralTypeConfiguration, IList<object>, NavigationPropertyConfiguration>(configuration,
                            bindingPath, nav));
                }
                else if (complex != null)
                {
                    StructuralTypeConfiguration complexType = builder.GetTypeConfigurationOrNull(complex.RelatedClrType) as StructuralTypeConfiguration;
                    builder.FindAllNavigationProperties(complexType, navs, path);
                }
                else if (collection != null)
                {
                    IEdmTypeConfiguration edmType = builder.GetTypeConfigurationOrNull(collection.ElementType);
                    if (edmType != null && edmType.Kind == EdmTypeKind.Complex)
                    {
                        StructuralTypeConfiguration complexType = (StructuralTypeConfiguration)edmType;

                        builder.FindAllNavigationProperties(complexType, navs, path);
                    }
                }

                path.Pop();
            }
        }
    }
}
