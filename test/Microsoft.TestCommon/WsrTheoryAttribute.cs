// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Microsoft.TestCommon
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class WsrTheoryAttribute : TheoryAttribute
    {
        public WsrTheoryAttribute()
        {
            Timeout = TimeoutConstant.DefaultTimeout;
            Platforms = Platform.All;
        }

        /// <summary>
        /// Gets the platform that the unit test is currently running on.
        /// </summary>
        protected Platform Platform
        {
            get { return PlatformInfo.Platform; }
        }

        /// <summary>
        /// Gets or set the platforms that the unit test is compatible with. Defaults to
        /// <see cref="Platform.All"/>.
        /// </summary>
        public Platform Platforms { get; set; }

        /// <inheritdoc/>
        protected override IEnumerable<ITestCommand> EnumerateTestCommands(IMethodInfo method)
        {
            if ((Platforms & Platform) == 0)
            {
                return new[] {
                    new SkipCommand(
                        method,
                        DisplayName,
                        String.Format("Unsupported platform (test only runs on {0}, current platform is {1})", Platforms, Platform)
                    )
                };
            }

            return base.EnumerateTestCommands(method);
        }
    }
}
