// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.ComponentModel;
using System.Web.Http.OData.Extensions;
using Microsoft.Data.OData;

namespace System.Web.Http
{
    /// <summary>
    /// Provides extension methods for the <see cref="HttpError"/> class.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ODataHttpErrorExtensions
    {
        /// <summary>
        /// Converts the <paramref name="httpError"/> to an <see cref="ODataError"/>.
        /// </summary>
        /// <param name="httpError">The <see cref="HttpError"/> instance to convert.</param>
        /// <returns>The converted <see cref="ODataError"/></returns>
        [Obsolete("This method is obsolete; use the CreateODataError method from the " + 
            "System.Web.Http.OData.Extensions or System.Web.OData.Extensions namespace.")]
        public static ODataError ToODataError(this HttpError httpError)
        {
            return HttpErrorExtensions.CreateODataError(httpError);
        }
    }
}
