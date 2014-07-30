// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;

namespace System.Web.OData
{
    internal sealed class MockAssembly : Assembly
    {
        Type[] _types;

        public MockAssembly(params Type[] types)
        {
            _types = types;
        }

        public MockAssembly(params MockType[] types)
        {
            foreach (var type in types)
            {
                type.SetupGet(t => t.Assembly).Returns(this);
            }
            _types = types.Select(t => t.Object).ToArray();
        }

        public override Type[] GetTypes()
        {
            return _types;
        }
    }
}
