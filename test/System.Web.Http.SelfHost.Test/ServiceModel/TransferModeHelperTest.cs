// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ServiceModel;
using System.Web.Http.SelfHost.ServiceModel;
using Microsoft.TestCommon;

namespace System.Net.Http.Formatting
{
    public class TransferModeHelperTest : EnumHelperTestBase<TransferMode>
    {
        public TransferModeHelperTest()
            : base(TransferModeHelper.IsDefined, TransferModeHelper.Validate, (TransferMode)999)
        {
        }
    }
}
