// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Net.Http.Headers;
using Microsoft.Data.OData;

namespace System.Web.Http.OData.Formatter
{
    /// <summary>
    /// Constants related to OData.
    /// </summary>
    internal static class ODataFormatterConstants
    {
        public const string DataServiceRelatedNamespace = "http://schemas.microsoft.com/ado/2007/08/dataservices/related/";
        public const string DefaultNamespace = "http://www.tempuri.org";

        public const string Element = "element";
        public const string Id = "Id";

        public const string Entry = "entry";
        public const string Feed = "feed";

        public const string ODataServiceVersion = "DataServiceVersion";
        public const string ODataMaxServiceVersion = "MaxDataServiceVersion";

        public const ODataVersion DefaultODataVersion = ODataVersion.V3;
        public static readonly ODataFormat DefaultODataFormat = ODataFormat.Atom;

        public const string DefaultApplicationODataMediaType = "application/atom+xml";
        private static readonly MediaTypeHeaderValue DefaultApplicationAtomXmlMediaType = new MediaTypeHeaderValue(DefaultApplicationODataMediaType);
        private static readonly MediaTypeHeaderValue DefaultApplicationJsonMediaType = MediaTypeHeaderValue.Parse("application/json;odata=verbose");
        private static readonly MediaTypeHeaderValue DefaultApplicationXmlMediaType = MediaTypeHeaderValue.Parse("application/xml");

        public static MediaTypeHeaderValue ApplicationAtomXmlMediaType
        {
            get { return (MediaTypeHeaderValue)((ICloneable)DefaultApplicationAtomXmlMediaType).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationJsonMediaType
        {
            get { return (MediaTypeHeaderValue)((ICloneable)DefaultApplicationJsonMediaType).Clone(); }
        }

        public static MediaTypeHeaderValue ApplicationXmlMediaType
        {
            get { return (MediaTypeHeaderValue)((ICloneable)DefaultApplicationXmlMediaType).Clone(); }
        }
    }
}
