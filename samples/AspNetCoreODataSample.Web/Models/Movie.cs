// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace AspNetCoreODataSample.Web.Models
{
    public class Movie
    {
        public int ID { get; set; }

        public List<MovieStar> Stars { get; set; }

        public string Title { get; set; }

        public DateTimeOffset ReleaseDate { get; set; }

        public Genre Genre { get; set; }

        public decimal Price { get; set; }
    }
}
