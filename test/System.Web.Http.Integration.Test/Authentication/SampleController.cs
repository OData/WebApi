// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Web.Http
{
    /// <summary>
    /// Sample ApiControler
    /// </summary>
    public class SampleController : ApiController
    {
        [RequireAdmin]
        public string Get()
        {
            return "hello";
        }
    }
}