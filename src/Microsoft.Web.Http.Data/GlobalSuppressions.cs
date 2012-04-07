// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Error List, point to "Suppress Message(s)", and click 
// "In Project Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Microsoft.Web.Http.Data.ChangeSetEntry.#OriginalAssociations")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Microsoft.Web.Http.Data.ChangeSetEntry.#Associations")]
[assembly: SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Scope = "member", Target = "Microsoft.Web.Http.Data.ChangeSetEntry.#EntityActions")]
[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "Assembly is delay signed")]
[assembly: SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Scope = "member", Target = "Microsoft.Web.Http.Data.ChangeSet.#SetEntityAssociations()")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "System.ComponentModel.DataAnnotations")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Scope = "type", Target = "Microsoft.Web.Http.Data.Metadata.MetadataProviderAttribute", Justification = "We intend for people to derive from this attribute.")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Web.Http.Data.Metadata", Justification = "These types are in their own namespace to match folder structure.")]
[assembly: SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "LookUp", Scope = "member", Target = "Microsoft.Web.Http.Data.Metadata.MetadataProvider.#LookUpIsEntityType(System.Type)", Justification = "This is intended to read as two words")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Microsoft.Web.Http.Data.SubmitActionDescriptor.#Execute(System.Web.Http.HttpControllerContext,System.Collections.Generic.IDictionary`2<System.String,System.Object>)", Justification = "This object is being returned - it can't be disposed.")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Microsoft.Web.Http.Data.DataControllerConfiguration.#CloneConfiguration(System.Web.Http.HttpConfiguration)", Justification = "This object cannot be disposed - it is being set on the execution context")]
[assembly: SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Scope = "member", Target = "Microsoft.Web.Http.Data.QueryFilterAttribute.#GetQueryResult`1(System.Linq.IQueryable`1<!!0>,System.Web.Http.Controllers.HttpActionContext)", Justification = "This object cannot be disposed - it is being returned as the result.")]
