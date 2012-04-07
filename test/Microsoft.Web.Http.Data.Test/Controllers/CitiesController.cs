// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.Web.Http.Data.Test.Models;

namespace Microsoft.Web.Http.Data.Test
{
    public class CitiesController : DataController
    {
        private CityData cityData = new CityData();

        public IQueryable<City> GetCities()
        {
            return this.cityData.Cities.AsQueryable();
        }
    }
}
