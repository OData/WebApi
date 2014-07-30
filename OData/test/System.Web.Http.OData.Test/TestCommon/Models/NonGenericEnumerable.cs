// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;

namespace System.Web.Http.OData.TestCommon.Models
{
    public class NonGenericEnumerable : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
