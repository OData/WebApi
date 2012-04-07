// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.CodeDom;
using System.Web.Compilation;

namespace System.Web.WebPages.Razor
{
    internal interface IAssemblyBuilder
    {
        void AddCodeCompileUnit(BuildProvider buildProvider, CodeCompileUnit compileUnit);
        void GenerateTypeFactory(string typeName);
    }
}
