//-----------------------------------------------------------------------------
// <copyright file="PublicApiTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace Microsoft.AspNet.OData.Test.PublicApi
{
    public partial class PublicApiTest
    {
        private const string AssemblyName = "Microsoft.AspNetCore.OData.dll";
        private const string OutputFileName = "Microsoft.AspNetCore.OData.PublicApi.out";
#if NETCOREAPP2_1
        private const string BaseLineFileName = "Microsoft.AspNetCore.OData.PublicApi.bsl";
#else
        private const string BaseLineFileName = "Microsoft.AspNetCore3x.OData.PublicApi.bsl";
#endif
        private const string BaseLineFileFolder = @"test\UnitTest\Microsoft.AspNetCore.OData.Test\PublicApi\";
    }
}
