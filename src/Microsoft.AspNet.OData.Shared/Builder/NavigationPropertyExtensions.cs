//-----------------------------------------------------------------------------
// <copyright file="NavigationPropertyExtensions.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.OData.Common;
using Microsoft.OData.Edm;

namespace Microsoft.AspNet.OData.Builder
{
    internal static class NavigationPropertyExtensions
    {
        public static void FindAllNavigationProperties(this ODataModelBuilder builder,
            StructuralTypeConfiguration configuration,
            IList<Tuple<StructuralTypeConfiguration, IList<MemberInfo>, NavigationPropertyConfiguration>> navigations,
            Stack<MemberInfo> path)
        {
            builder.FindAllNavigationPropertiesRecursive(configuration, navigations, path, new HashSet<Type>());
        }

        private static void FindAllNavigationPropertiesRecursive(this ODataModelBuilder builder,
            StructuralTypeConfiguration configuration,
            IList<Tuple<StructuralTypeConfiguration, IList<MemberInfo>, NavigationPropertyConfiguration>> navigations,
            Stack<MemberInfo> path,
            HashSet<Type> typesAlreadyProcessed)
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
                builder.FindNavigationProperties(config, navigations, path, typesAlreadyProcessed);
            }

            IEnumerable<StructuralTypeConfiguration> derivedTypes = builder.DerivedTypes(configuration);
            foreach (var config in derivedTypes)
            {
                if (path.OfType<Type>().Any(p => p == config.ClrType))
                {
                    continue;
                }

                path.Push(TypeHelper.AsMemberInfo(config.ClrType));

                builder.FindNavigationProperties(config, navigations, path, typesAlreadyProcessed);

                path.Pop();
            }
        }

        private static void FindNavigationProperties(this ODataModelBuilder builder, StructuralTypeConfiguration configuration,
            IList<Tuple<StructuralTypeConfiguration, IList<MemberInfo>, NavigationPropertyConfiguration>> navs,
            Stack<MemberInfo> path, HashSet<Type> typesAlreadyProcessed)
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
                    IList<MemberInfo> bindingPath = path.Reverse().ToList();

                    navs.Add(
                        new Tuple<StructuralTypeConfiguration, IList<MemberInfo>, NavigationPropertyConfiguration>(configuration,
                            bindingPath, nav));
                }
                else if (complex != null && !typesAlreadyProcessed.Contains(complex.RelatedClrType))
                {
                    StructuralTypeConfiguration complexType = builder.GetTypeConfigurationOrNull(complex.RelatedClrType) as StructuralTypeConfiguration;

                    // Prevent infinite recursion on self-referential complex types.
                    typesAlreadyProcessed.Add(complex.RelatedClrType);
                    builder.FindAllNavigationPropertiesRecursive(complexType, navs, path, typesAlreadyProcessed);
                    typesAlreadyProcessed.Remove(complex.RelatedClrType);
                }
                else if (collection != null && !typesAlreadyProcessed.Contains(collection.ElementType))
                {
                    IEdmTypeConfiguration edmType = builder.GetTypeConfigurationOrNull(collection.ElementType);
                    if (edmType != null && edmType.Kind == EdmTypeKind.Complex)
                    {
                        StructuralTypeConfiguration complexType = (StructuralTypeConfiguration)edmType;

                        // Prevent infinite recursion on self-referential complex types.
                        typesAlreadyProcessed.Add(collection.ElementType);
                        builder.FindAllNavigationPropertiesRecursive(complexType, navs, path, typesAlreadyProcessed);
                        typesAlreadyProcessed.Remove(collection.ElementType);
                    }
                }

                path.Pop();
            }
        }
    }
}
