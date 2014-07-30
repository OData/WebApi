// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Net.Http", Justification = "Follows System.Net.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.OData.Formatter", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.OData.Formatter.Serialization", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.OData.Builder.Conventions.Attributes", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.OData.Results", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "These assemblies are delay-signed.")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "System.Web.OData.Formatter.EdmLibHelpers.#.cctor()", Justification = "Class coupling necessary in this class")]
[assembly: SuppressMessage("Microsoft.Web.FxCop", "MW1000:UnusedResourceUsageRule", MessageId = "172567", Justification = "Resource used by framework")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "orderby", Scope = "resource", Target = "System.Web.OData.Properties.SRResources.resources", Justification = "$orderby is an odata keyword")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "it", Scope = "resource", Target = "System.Web.OData.Properties.SRResources.resources", Justification = "$it is an odata keyword")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "type", Target = "System.Web.OData.Builder.EdmModelHelperMethods", Justification = "Static helper class. Class coupling acceptable here.")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "type", Target = "System.Web.OData.Formatter.EdmLibHelpers", Justification = "Static helper class. Class coupling acceptable here.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unsortable", Scope = "member", Target = "System.Web.OData.Builder.PropertyConfiguration.#Unsortable", Justification = "spelled correctly")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unsortable", Scope = "member", Target = "System.Web.OData.Builder.PropertyConfiguration.#IsUnsortable()", Justification = "spelled correctly")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unsortable", Scope = "member", Target = "System.Web.OData.QueryableRestrictions.#Unsortable", Justification = "spelled correctly")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Unsortable", Scope = "type", Target = "System.Web.OData.Query.UnsortableAttribute", Justification = "spelled correctly")]
