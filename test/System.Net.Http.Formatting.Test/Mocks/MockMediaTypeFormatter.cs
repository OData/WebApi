// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http.Formatting.Mocks
{
    public class MockMediaTypeFormatter : MediaTypeFormatter
    {
        public bool CallBase { get; set; }
        public Func<Type, bool> CanReadTypeCallback { get; set; }
        public Func<Type, bool> CanWriteTypeCallback { get; set; }

        public override bool CanReadType(Type type)
        {
            if (!CallBase && CanReadTypeCallback == null)
            {
                throw new InvalidOperationException("CallBase or CanReadTypeCallback must be set first.");
            }

            return CanReadTypeCallback != null ? CanReadTypeCallback(type) : true;
        }

        public override bool CanWriteType(Type type)
        {
            if (!CallBase && CanWriteTypeCallback == null)
            {
                throw new InvalidOperationException("CallBase or CanWriteTypeCallback must be set first.");
            }

            return CanWriteTypeCallback != null ? CanWriteTypeCallback(type) : true;
        }

        new public Encoding SelectCharacterEncoding(HttpContentHeaders contentHeaders)
        {
            return base.SelectCharacterEncoding(contentHeaders);
        }
    }
}
