﻿// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;

namespace WebApiPerformance.Service
{
    public class WebApiJsonController : ApiController
    {
        [HttpGet]
        public IEnumerable<ClassA> Get()
        {
            int n;
            n =
                int.TryParse(
                    Request.GetQueryNameValuePairs().Where(kv => kv.Key == "n").Select(kv => kv.Value).FirstOrDefault(), out n)
                    ? n
                    : 10;
            return TestRepo.GetAs(n);
        }
    }
}