// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Web.Http.Properties;

namespace System.Web.Http.Query
{
    [SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification = "Exception used only internally.")]
    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic", Justification = "Exception used only internally.")]
    [SuppressMessage("Microsoft.Usage", "CA2237:MarkISerializableTypesWithSerializable", Justification = "Exception used only internally.")]
    internal class ParseException : Exception
    {
        public ParseException(string message, int position)
            : base(string.Format(CultureInfo.CurrentCulture, SRResources.ParseExceptionFormat, message, position))
        {
        }

        public ParseException(string message)
            : base(message)
        {
        }
    }
}
