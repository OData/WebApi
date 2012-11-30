// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// Contains media types used by the OData formatter.
    /// </summary>
    internal static class ODataMediaTypes
    {
        private static readonly MediaTypeHeaderValue _applicationAtomSvcXml =
            new MediaTypeHeaderValue("application/atomsvc+xml");
        private static readonly MediaTypeHeaderValue _applicationAtomXml =
            new MediaTypeHeaderValue("application/atom+xml");
        private static readonly MediaTypeHeaderValue _applicationAtomXmlTypeEntry =
            MediaTypeHeaderValue.Parse("application/atom+xml;type=entry");
        private static readonly MediaTypeHeaderValue _applicationAtomXmlTypeFeed =
            MediaTypeHeaderValue.Parse("application/atom+xml;type=feed");
        private static readonly MediaTypeHeaderValue _applicationJsonODataVerbose =
            MediaTypeHeaderValue.Parse("application/json;odata=verbose");
        private static readonly MediaTypeHeaderValue _applicationXml = new MediaTypeHeaderValue("application/xml");
        private static readonly MediaTypeHeaderValue _textXml = new MediaTypeHeaderValue("text/xml");

        public static MediaTypeHeaderValue ApplicationAtomSvcXml
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationAtomSvcXml).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationAtomXml
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationAtomXml).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationAtomXmlTypeEntry
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationAtomXmlTypeEntry).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationAtomXmlTypeFeed
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationAtomXmlTypeFeed).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonODataVerbose
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationJsonODataVerbose).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationXml
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_applicationXml).Clone(); }
        }

        public static MediaTypeHeaderValue TextXml
        {
            get { return (MediaTypeHeaderValue)((ICloneable)_textXml).Clone(); }
        }
    }
}
