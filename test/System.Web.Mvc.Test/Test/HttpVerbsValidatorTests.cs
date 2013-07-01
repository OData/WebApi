// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.TestCommon;

namespace System.Web.Mvc.Test
{
    public class HttpVerbsValidatorTests
    {
        [Fact]
        public void EnumToArray()
        {
            // Arrange
            IDictionary<string, HttpVerbs> enumValues = EnumToDictionary<HttpVerbs>();
            var allCombinations = EnumerableToCombinations(enumValues);

            // Act & assert
            foreach (var combination in allCombinations)
            {
                // generate all the names + values in this combination
                List<string> aggrNames = new List<string>();
                HttpVerbs aggrValues = (HttpVerbs)0;
                foreach (var entry in combination)
                {
                    aggrNames.Add(entry.Key);
                    aggrValues |= entry.Value;
                }

                // get the resulting array
                string[] array = HttpVerbsValidator.EnumToArray(aggrValues);
                var aggrNamesOrdered = aggrNames.OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
                var arrayOrdered = array.OrderBy(name => name, StringComparer.OrdinalIgnoreCase);
                bool match = aggrNamesOrdered.SequenceEqual(arrayOrdered, StringComparer.OrdinalIgnoreCase);

                if (!match)
                {
                    string invalidEnumFormatString = @"The enum '{0}' did not produce the correct array.
Expected: {1}
Actual: {2}";
                    string message = String.Format(invalidEnumFormatString, aggrValues,
                                                   aggrNames.Aggregate((a, b) => a + ", " + b),
                                                   array.Aggregate((a, b) => a + ", " + b));
                    Assert.True(false, message);
                }
            }
        }

        private static IDictionary<string, TEnum> EnumToDictionary<TEnum>()
        {
            // Arrange
            var values = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();
            return values.ToDictionary(value => Enum.GetName(typeof(TEnum), value), value => value);
        }

        private static IEnumerable<ICollection<T>> EnumerableToCombinations<T>(IEnumerable<T> elements)
        {
            List<T> allElements = elements.ToList();

            int maxCount = 1 << allElements.Count;
            for (int idxCombination = 0; idxCombination < maxCount; idxCombination++)
            {
                List<T> thisCollection = new List<T>();
                for (int idxBit = 0; idxBit < 32; idxBit++)
                {
                    bool bitActive = (((uint)idxCombination >> idxBit) & 1) != 0;
                    if (bitActive)
                    {
                        thisCollection.Add(allElements[idxBit]);
                    }
                }
                yield return thisCollection;
            }
        }

    }
}