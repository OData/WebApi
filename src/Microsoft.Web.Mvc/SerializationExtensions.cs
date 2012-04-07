// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Web.Mvc;
using Microsoft.Web.Mvc.Properties;

namespace Microsoft.Web.Mvc
{
    public static class SerializationExtensions
    {
        public static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name)
        {
            return Serialize(htmlHelper, name, MvcSerializer.DefaultSerializationMode);
        }

        public static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name, SerializationMode mode)
        {
            return SerializeInternal(htmlHelper, name, null, mode, true /* useViewData */);
        }

        internal static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name, SerializationMode mode, MvcSerializer serializer)
        {
            return SerializeInternal(htmlHelper, name, null, mode, true /* useViewData */, serializer);
        }

        public static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name, object data)
        {
            return Serialize(htmlHelper, name, data, MvcSerializer.DefaultSerializationMode);
        }

        public static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name, object data, SerializationMode mode)
        {
            return SerializeInternal(htmlHelper, name, data, mode, false /* useViewData */);
        }

        internal static MvcHtmlString Serialize(this HtmlHelper htmlHelper, string name, object data, SerializationMode mode, MvcSerializer serializer)
        {
            return SerializeInternal(htmlHelper, name, data, mode, false /* useViewData */, serializer);
        }

        private static MvcHtmlString SerializeInternal(HtmlHelper htmlHelper, string name, object data, SerializationMode mode, bool useViewData)
        {
            return SerializeInternal(htmlHelper, name, data, mode, useViewData, null);
        }

        private static MvcHtmlString SerializeInternal(HtmlHelper htmlHelper, string name, object data, SerializationMode mode, bool useViewData, MvcSerializer serializer)
        {
            if (htmlHelper == null)
            {
                throw new ArgumentNullException("htmlHelper");
            }

            if (String.IsNullOrEmpty(name))
            {
                throw new ArgumentException(MvcResources.Common_NullOrEmpty, "name");
            }

            name = htmlHelper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(name);
            if (useViewData)
            {
                data = htmlHelper.ViewData.Eval(name);
            }

            string serializedData = (serializer ?? new MvcSerializer()).Serialize(data, mode);

            TagBuilder builder = new TagBuilder("input");
            builder.Attributes["type"] = "hidden";
            builder.Attributes["name"] = name;
            builder.Attributes["value"] = serializedData;
            return MvcHtmlString.Create(builder.ToString(TagRenderMode.SelfClosing));
        }
    }
}
