//-----------------------------------------------------------------------------
// <copyright file="MetadataTestHelpers.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;
using Microsoft.Test.E2E.AspNet.OData.Formatter.JsonLight.Metadata.Model;

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
            var results = InstanceCreator.CreateInstanceOf<T>(new Random(RandomSeedGenerator.GetRandomSeed()), new CreatorSettings { NullValueProbability = 0, AllowEmptyCollection = false });

            return results;
        }

        /// <summary>
        /// Returns a fixed, deterministic set of <see cref="EntityWithSimpleProperties"/> instances.
        /// Both the controller (server-side data) and the tests (client-side expectations) call this so the
        /// two always agree, regardless of when each one runs.
        /// Using the random <see cref="CreateInstances{T}"/> here made the tests flaky: the array length
        /// depends on <see cref="RandomSeedGenerator.GetRandomSeed"/>, which changes every hour, and the
        /// controller's process-wide static in-memory table accumulates entities across those hourly seeds.
        /// When the client and server calls landed in different hours the expected and actual feed lengths
        /// diverged (e.g. Expected 5 / Actual 6). This mirrors the deterministic data already used by
        /// <see cref="Controllers.BaseEntityController"/>.
        /// </summary>
        public static EntityWithSimpleProperties[] GetEntityWithSimplePropertiesInstances()
        {
            var entities = new EntityWithSimpleProperties[5];
            for (int i = 1; i <= entities.Length; i++)
            {
                entities[i - 1] = new EntityWithSimpleProperties
                {
                    Id = i,
                    NullableIntProperty = i * 10,
                    BinaryProperty = new byte[] { (byte)i, (byte)(i + 1), (byte)(i + 2) },
                    BooleanProperty = (i % 2) == 0,
                    DurationProperty = TimeSpan.FromMinutes(i),
                    DecimalProperty = i + 0.5m,
                    DoubleProperty = i + 0.25,
                    SingleProperty = i + 0.75f,
                    GuidProperty = Guid.Parse("00000000-0000-0000-0000-00000000000" + i),
                    Int16Property = (short)i,
                    Int32Property = i,
                    Int64Property = i,
                    SbyteProperty = (sbyte)i,
                    DateTimeOffsetProperty = new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(i),
                    StringProperty = "Entity" + i,
                    EnumerationProperty = SimpleEnumeration.FirstValue
                };
            }

            return entities;
        }
    }
}
