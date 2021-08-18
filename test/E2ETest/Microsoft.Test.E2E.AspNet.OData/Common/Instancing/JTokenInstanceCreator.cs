//-----------------------------------------------------------------------------
// <copyright file="JTokenInstanceCreator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    internal class JTokenInstanceCreator
    {
        private const int MaxDepth = 3;

        internal static JToken CreateInstanceOfJToken(Random rndGen, CreatorSettings creatorSettings, int currDepth = 0)
        {
            var depth = rndGen.Next(MaxDepth);
            if (currDepth >= depth)
            {
                return CreateInstanceOfJValue(rndGen, creatorSettings);
            }
            else
            {
                switch (rndGen.Next(3))
                {
                    case 0:
                        return CreateInstanceOfJValue(rndGen, creatorSettings);
                    case 1:
                        return CreateInstanceOfJArray(rndGen, creatorSettings);
                    default:
                        return CreateInstanceOfJObject(rndGen, creatorSettings);
                }
            }
        }

        internal static JValue CreateInstanceOfJValue(Random rndGen, CreatorSettings creatorSettings)
        {
            var rnd = rndGen.Next(20);

            object value;
            switch (rnd)
            {
                case 0:
                case 1:
                    value = InstanceCreator.CreateInstanceOf<bool>(rndGen, creatorSettings);
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    value = InstanceCreator.CreateInstanceOf<int>(rndGen, creatorSettings);
                    break;
                case 7:
                case 8:
                // workaround for Bug371740
                //value = InstanceCreator.CreateInstanceOf<double>(rndGen, creatorSettings);
                //break;
                case 9:
                    value = InstanceCreator.CreateInstanceOf<DateTime>(rndGen, creatorSettings);
                    break;
                case 10:
                    value = InstanceCreator.CreateInstanceOf<TimeSpan>(rndGen, creatorSettings);
                    break;
                case 11:
                    value = InstanceCreator.CreateInstanceOf<Guid>(rndGen, creatorSettings);
                    break;
                case 12:
                    value = InstanceCreator.CreateInstanceOf<Uri>(rndGen, creatorSettings);
                    break;
                default:
                    value = InstanceCreator.CreateInstanceOf<string>(rndGen, creatorSettings);
                    break;
            }

            return new JValue(value);
        }

        internal static JArray CreateInstanceOfJArray(Random rndGen, CreatorSettings creatorSettings, int currDepth = 0)
        {
            var arr = new JArray();
            var max = rndGen.Next(creatorSettings.MaxArrayLength);
            for (int i = 0; i < max; i++)
            {
                arr.Add(CreateInstanceOfJToken(rndGen, creatorSettings, currDepth + 1));
            }
            return arr;
        }

        internal static JObject CreateInstanceOfJObject(Random rndGen, CreatorSettings creatorSettings, int currDepth = 0)
        {
            var obj = new JObject();

            var existingPropertyName = new HashSet<string>();
            var maxPropCount = rndGen.Next(creatorSettings.MaxListLength);

            int currPropCount = 0;
            while (currPropCount < maxPropCount)
            {
                var propertyName = InstanceCreator.CreateInstanceOf<string>(rndGen, new CreatorSettings(creatorSettings) { NullValueProbability = 0.0 });
                if (!existingPropertyName.Contains(propertyName))
                {
                    obj.Add(propertyName, CreateInstanceOfJToken(rndGen, creatorSettings, currDepth + 1));

                    existingPropertyName.Add(propertyName);
                    currPropCount++;
                }
            }

            return obj;
        }
    }
}
