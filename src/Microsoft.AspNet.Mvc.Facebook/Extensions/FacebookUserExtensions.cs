// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
#if Debug
using System.Dynamic;
using System.Text;
using System.Web;
using Microsoft.AspNet.Mvc.Facebook.Models;

namespace Microsoft.AspNet.Mvc.Facebook.Extensions
{
    public static class FacebookUserExtensions
    {
        public static IHtmlString ToHtml(this FacebookUser user)
        {
            var results = new StringBuilder();

            RenderJson(user.Data, results);

            return new HtmlString(results.ToString());
        }

        private static void RenderJson(dynamic json, StringBuilder results)
        {
            if (json != null)
            {
                results.Append("<ul>");
                foreach (var item in json)
                {
                    results.Append("<li>");
                    results.Append(item.Key);
                    if (item.Value == null)
                    {
                        results.Append(" : null");
                    }
                    else
                    {
                        if (item.Value is DynamicObject)
                        {
                            RenderJson(item.Value, results);
                        }
                        else
                        {
                            results.Append(" : ");
                            if (item.Value is string)
                            {
                                results.AppendFormat("\"{0}\"", item.Value);
                            }
                            else
                            {
                                results.Append(item.Value);
                            }
                        }
                    }
                    results.Append("</li>");
                }
                results.Append("</ul>");
            }
        }
    }
}
#endif