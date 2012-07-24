// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Web.Razor.Tokenizer.Symbols;

namespace System.Web.Razor.Tokenizer
{
    public interface ITokenizer
    {
        ISymbol NextSymbol();
    }
}
