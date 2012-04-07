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

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames", Justification = "Assembly is delay signed")]
[assembly: SuppressMessage("Microsoft.Design", "CA1020:AvoidNamespacesWithFewTypes", Scope = "namespace", Target = "Microsoft.Web.Http.Data.Helpers", Justification = "There are just a few helpers for client generation.")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Web.Http.Data.TypeDescriptorExtensions.#ContainsAttributeType`1(System.ComponentModel.AttributeCollection)", Justification = "Used in Microsoft.Web.Http.Data assembly")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Web.Http.Data.TypeUtility.#GetKnownTypes(System.Type,System.Boolean)", Justification = "Used in Microsoft.Web.Http.Data assembly")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member", Target = "Microsoft.Web.Http.Data.TypeUtility.#UnwrapTaskInnerType(System.Type)", Justification = "Used in Microsoft.Web.Http.Data assembly")]
