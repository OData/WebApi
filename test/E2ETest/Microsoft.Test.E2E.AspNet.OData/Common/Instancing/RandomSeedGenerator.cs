//-----------------------------------------------------------------------------
// <copyright file="RandomSeedGenerator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Globalization;
using System.IO;
using System.Security;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    public static class RandomSeedGenerator
    {
        #region Constants and Fields

        private const string SeedOverrideFileName = @"C:\RandomSeed.txt";

        #endregion

        #region Public Methods and Operators

        public static int GetRandomSeed()
        {
            try
            {
                if (File.Exists(SeedOverrideFileName))
                {
                    string contents = File.ReadAllText(SeedOverrideFileName).Trim();
                    int possibleSeed;
                    if (int.TryParse(contents, NumberStyles.None, CultureInfo.InvariantCulture, out possibleSeed))
                    {
                        return possibleSeed;
                    }
                }
            }
            catch (IOException)
            {
                // ignore exceptions if cannot read the file, simply use the current date as the seed
            }
            catch (SecurityException)
            {
            }

            // Keep the randomness per hour instead of per day.
            DateTime now = DateTime.UtcNow;
            int seed = (now.Year * 10000) + (now.Month * 100) + now.Day + now.Hour;
            return seed;
        }

        #endregion
    }
}
