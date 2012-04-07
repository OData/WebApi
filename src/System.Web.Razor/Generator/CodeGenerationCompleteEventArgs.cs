// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.CodeDom;
using Microsoft.Internal.Web.Utils;

namespace System.Web.Razor.Generator
{
    public class CodeGenerationCompleteEventArgs : EventArgs
    {
        public CodeGenerationCompleteEventArgs(string virtualPath, string physicalPath, CodeCompileUnit generatedCode)
        {
            if (String.IsNullOrEmpty(virtualPath))
            {
                throw ExceptionHelper.CreateArgumentNullOrEmptyException("virtualPath");
            }
            if (generatedCode == null)
            {
                throw new ArgumentNullException("generatedCode");
            }
            VirtualPath = virtualPath;
            PhysicalPath = physicalPath;
            GeneratedCode = generatedCode;
        }

        public CodeCompileUnit GeneratedCode { get; private set; }
        public string VirtualPath { get; private set; }
        public string PhysicalPath { get; private set; }
    }
}
