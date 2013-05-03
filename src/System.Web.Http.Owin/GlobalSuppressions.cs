// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Design", "CA2210:AssembliesShouldHaveValidStrongNames",
    Justification = "The assembly is delay signed")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
    Target = "System.Collections.Generic.DictionaryExtensions.#RemoveFromDictionary`2(System.Collections.Generic.IDictionary`2<!!0,!!1>,System.Func`2<System.Collections.Generic.KeyValuePair`2<!!0,!!1>,System.Boolean>)",
    Justification = "The shared source file is used by other assemblies.")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
    Target = "System.Collections.Generic.DictionaryExtensions.#RemoveFromDictionary`3(System.Collections.Generic.IDictionary`2<!!0,!!1>,System.Func`3<System.Collections.Generic.KeyValuePair`2<!!0,!!1>,!!2,System.Boolean>,!!2)",
    Justification = "The shared source file is used by other assemblies.")]
[assembly: SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Scope = "member",
    Target = "System.Collections.Generic.DictionaryExtensions.#FindKeysWithPrefix`1(System.Collections.Generic.IDictionary`2<System.String,!!0>,System.String)",
    Justification = "The shared source file is used by other assemblies.")]
