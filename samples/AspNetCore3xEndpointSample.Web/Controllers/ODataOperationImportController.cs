// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using Microsoft.AspNet.OData;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCore3xEndpointSample.Web.Controllers
{
    public class ODataOperationImportController : ODataController
    {
        [HttpGet]
        public IActionResult CalcByRating([FromODataUri]int order)
        {
            return Ok($"In CalcByRating using Order = {order}");
        }

        [HttpGet]
        public IActionResult CalcByRating([FromODataUri]string name)
        {
            return Ok($"In CalcByRating using name = {name}");
        }

        [HttpPost]
        public IActionResult CalcByRatingAction(ODataActionParameters parameters)
        {
            if (parameters == null)
            {
                return Ok($"In CalcByRating Action Null parameters");
            }

            if (parameters.Count == 0)
            {
                return Ok($"In CalcByRating Action Empty parameters");
            }

            if (parameters.ContainsKey("order"))
            {
                return Ok($"In CalcByRating Action order = {parameters["order"]}");
            }
            else
            {
                return Ok($"In CalcByRating Action without order value");
            }
        }

    }
}
