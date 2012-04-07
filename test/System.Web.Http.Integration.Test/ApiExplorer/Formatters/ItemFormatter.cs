// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Formatting;

namespace System.Web.Http.ApiExplorer
{
    public class ItemFormatter : BufferedMediaTypeFormatter
    {
        public override bool CanReadType(Type type)
        {
            return typeof(System.Web.Http.ApiExplorer.ItemController.Item).IsAssignableFrom(type);
        }

        public override bool CanWriteType(Type type)
        {
            return typeof(System.Web.Http.ApiExplorer.ItemController.Item).IsAssignableFrom(type);
        }

        public override object ReadFromStream(Type type, IO.Stream stream, Net.Http.Headers.HttpContentHeaders contentHeaders, IFormatterLogger formatterLogger)
        {
            return base.ReadFromStream(type, stream, contentHeaders, formatterLogger);
        }

        public override void WriteToStream(Type type, object value, IO.Stream stream, Net.Http.Headers.HttpContentHeaders contentHeaders)
        {
            base.WriteToStream(type, value, stream, contentHeaders);
        }
    }
}
