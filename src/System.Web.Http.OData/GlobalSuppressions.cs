// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.OData.Formatter", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.OData.Formatter.Serialization", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.OData.Builder.Conventions.Attributes", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.Web.Http.OData.Results", Justification = "Follows System.Web.Http naming")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "type", Target = "System.Web.Http.OData.Formatter.EdmLibHelpers", Justification = "Mostly extension methods")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Scope = "member", Target = "System.Web.Http.OData.Formatter.EdmLibHelpers.#.cctor()", Justification = "Class coupling necessary in this class")]
[assembly: SuppressMessage("Microsoft.Web.FxCop", "MW1000:UnusedResourceUsageRule", MessageId = "172567", Justification = "Resource used by framework")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "inlinecount", Scope = "resource", Target = "System.Web.Http.OData.Properties.SRResources.resources", Justification = "spelled correctly")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "orderby", Scope = "resource", Target = "System.Web.Http.OData.Properties.SRResources.resources", Justification = "$orderby is an odata keyword")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1703:ResourceStringsShouldBeSpelledCorrectly", MessageId = "it", Scope = "resource", Target = "System.Web.Http.OData.Properties.SRResources.resources", Justification = "$it is an odata keyword")]
