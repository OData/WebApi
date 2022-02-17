//-----------------------------------------------------------------------------
// <copyright file="MetadataTestHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;

namespace Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata
{
    public static class MetadataTestHelpers
    {
        public static void SetAcceptHeader(this HttpRequestMessage message, string acceptHeader)
        {
            message.Headers.Clear();
            message.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(acceptHeader));
        }

        public static T CreateInstances<T>()
        {
            var seed = RandomSeedGenerator.GetRandomSeed();
            Trace.WriteLine($"Generated seed for random number generator: {seed}");

            var random = new Random(seed);

            var results = InstanceCreator.CreateInstanceOf<T>(random, new CreatorSettings { NullValueProbability = 0, AllowEmptyCollection = false });

            return results;
        }
    }
}
