//-----------------------------------------------------------------------------
// <copyright file="MovieStar.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

namespace AspNetCoreODataSample.Web.Models
{
    public class MovieStar
    {
        public Movie Movie { get; set; }

        public int MovieId { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
    }
}
