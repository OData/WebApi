// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

namespace System.Json
{
    internal static class Log
    {
        public static void Info(string text, params object[] args)
        {
            Console.WriteLine(text, args);
        }
    }
}
