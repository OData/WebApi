// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.TestCommon;

namespace System.Web.Http.OData.Query
{
    [PartialTrustRunner]
    public class PartialTrustTest : QueryCompositionTests
    {
        [Fact(Skip="Moq doesn't work in partial trust")]
        public override void QueryableUsesConfiguredAssembliesResolver()
        {
            throw new NotImplementedException();
        }
    }
}
