// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System.Collections;
using System.Collections.Generic;

namespace AspNetCore3xEndpointSample.Web.Models
{
    public static class EdmModelBuilder
    {
        private static IEdmModel _edmModel;

        public static IEdmModel GetEdmModel()
        {
            if (_edmModel == null)
            {
                var builder = new ODataConventionModelBuilder();
                builder.EntitySet<Customer>("Customers");
                builder.EntitySet<Order>("Orders");

                builder.EntitySet<ImageAd>("ImageAds");
                builder.EntitySet<Image>("Images");
                _edmModel = builder.GetEdmModel();
            }

            return _edmModel;
        }

    }

    public class ImageAd
    {
        public int Id { get; set; }

        [AutoExpand]
        public IList<Image> Images { get; set; }

        //public IEnumerable<Image> Images { get; set; }
    }

    public class SubImageAd : ImageAd
    {
        [AutoExpand]
        public IList<Image> SubImages { get; set; }

        //public IEnumerable<Image> Images { get; set; }
    }

    public class Image
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}