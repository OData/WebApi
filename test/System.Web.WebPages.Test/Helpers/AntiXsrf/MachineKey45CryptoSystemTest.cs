// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using Xunit;
using Assert = Microsoft.TestCommon.AssertEx;

namespace System.Web.Helpers.AntiXsrf.Test
{
    public class MachineKey45CryptoSystemTest
    {
        [Fact]
        public void Protect()
        {
            // Arrange
            byte[] expectedInputBytes = new byte[] { 1, 2, 3, 4, 5 };
            byte[] expectedOutputBytes = new byte[] { 6, 7, 8, 9, 10 };
            string expectedOutputString = HttpServerUtility.UrlTokenEncode(expectedOutputBytes);

            Func<byte[], string[], byte[]> protectThunk = (input, purposes) =>
            {
                Assert.Equal(expectedInputBytes, input);
                Assert.Equal(new string[] { "System.Web.Helpers.AntiXsrf.AntiForgeryToken.v1" }, purposes);
                return expectedOutputBytes;
            };

            MachineKey45CryptoSystem cryptoSystem = new MachineKey45CryptoSystem(protectThunk, null);

            // Act
            string output = cryptoSystem.Protect(expectedInputBytes);

            // Assert
            Assert.Equal(expectedOutputString, output);
        }

        [Fact]
        public void Unprotect()
        {
            // Arrange
            byte[] expectedInputBytes = new byte[] { 1, 2, 3, 4, 5 };
            string expectedInputString = HttpServerUtility.UrlTokenEncode(expectedInputBytes);
            byte[] expectedOutputBytes = new byte[] { 6, 7, 8, 9, 10 };

            Func<byte[], string[], byte[]> unprotectThunk = (input, purposes) =>
            {
                Assert.Equal(expectedInputBytes, input);
                Assert.Equal(new string[] { "System.Web.Helpers.AntiXsrf.AntiForgeryToken.v1" }, purposes);
                return expectedOutputBytes;
            };

            MachineKey45CryptoSystem cryptoSystem = new MachineKey45CryptoSystem(null, unprotectThunk);

            // Act
            byte[] output = cryptoSystem.Unprotect(expectedInputString);

            // Assert
            Assert.Equal(expectedOutputBytes, output);
        }
    }
}
