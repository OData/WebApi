// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.CodeDom;
using System.Web.UI;

namespace System.Web.Mvc
{
    internal sealed class ViewMasterPageControlBuilder : FileLevelMasterPageControlBuilder, IMvcControlBuilder
    {
        public string Inherits { get; set; }

        public override void ProcessGeneratedCode(CodeCompileUnit codeCompileUnit, CodeTypeDeclaration baseType, CodeTypeDeclaration derivedType, CodeMemberMethod buildMethod, CodeMemberMethod dataBindingMethod)
        {
            if (!String.IsNullOrWhiteSpace(Inherits))
            {
                derivedType.BaseTypes[0] = new CodeTypeReference(Inherits);
            }
        }
    }
}
