//-----------------------------------------------------------------------------
// <copyright file="ODataOptionsTest.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using Microsoft.AspNet.OData.Test.Common;
using Xunit;

namespace Microsoft.AspNet.OData.Test
{
#if NETCORE
    public class ODataOptionsTest
    {
        [Fact]
        public void MaxReceivedMessageSize_DefaultIsOneHundredMB()
        {
            // Arrange & Act
            ODataOptions options = new ODataOptions();

            // Assert
            Assert.Equal(ODataMessageSizeOptions.DefaultMaxReceivedMessageSize, options.MaxReceivedMessageSize);
        }

        [Fact]
        public void MaxReceivedMessageSize_SetAndGet()
        {
            // Arrange
            ODataOptions options = new ODataOptions();
            long customSize = 50 * 1024 * 1024;

            // Act
            options.MaxReceivedMessageSize = customSize;

            // Assert
            Assert.Equal(customSize, options.MaxReceivedMessageSize);
        }

        [Fact]
        public void MaxReceivedMessageSize_Throws_ForZero()
        {
            // Arrange
            ODataOptions options = new ODataOptions();

            // Act & Assert
            ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => options.MaxReceivedMessageSize = 0);
        }
    }
#endif


    public class ODataMessageSizeOptionsTest
    {
        [Fact]
        public void MaxReceivedMessageSize_DefaultIsOneHundredMB()
        {
            // Arrange & Act
            ODataMessageSizeOptions options = new ODataMessageSizeOptions();

            Assert.Equal(100L * 1024 * 1024, ODataMessageSizeOptions.DefaultMaxReceivedMessageSize);
            Assert.Equal(ODataMessageSizeOptions.DefaultMaxReceivedMessageSize, options.MaxReceivedMessageSize);
        }

        [Fact]
        public void MaxReceivedMessageSize_SetAndGet()
        {
            // Arrange
            ODataMessageSizeOptions options = new ODataMessageSizeOptions();
            long customSize = 50 * 1024 * 1024;

            // Act
            options.MaxReceivedMessageSize = customSize;

            // Assert
            Assert.Equal(customSize, options.MaxReceivedMessageSize);
        }

        [Fact]
        public void MaxReceivedMessageSize_Throws_ForZero()
        {
            // Arrange
            ODataMessageSizeOptions options = new ODataMessageSizeOptions();

            // Act & Assert
            ArgumentOutOfRangeException exception = ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => options.MaxReceivedMessageSize = 0);
            Assert.Contains("Value must be greater than or equal to 1", exception.Message);
        }

        [Fact]
        public void MaxReceivedMessageSize_Throws_ForNegative()
        {
            // Arrange
            ODataMessageSizeOptions options = new ODataMessageSizeOptions();

            // Act & Assert
            ArgumentOutOfRangeException exception = ExceptionAssert.Throws<ArgumentOutOfRangeException>(
                () => options.MaxReceivedMessageSize = -1);
            Assert.Contains("Value must be greater than or equal to 1", exception.Message);
        }

        [Fact]
        public void MaxReceivedMessageSize_AcceptsMinimumValue()
        {
            // Arrange
            ODataMessageSizeOptions options = new ODataMessageSizeOptions();

            // Act
            options.MaxReceivedMessageSize = 1;

            // Assert
            Assert.Equal(1, options.MaxReceivedMessageSize);
        }
    }
}
