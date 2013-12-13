// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Web.Http.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.Http.OData.Routing
{
    /// <summary>
    /// Parses the OData multi-key value (same for function call parameters) as a dictionary.
    /// </summary>
    internal static class KeyValueParser
    {
        public static Dictionary<string, string> ParseKeys(string segment)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            int currentIndex = 0;
            int startIndex = 0;

            while (currentIndex < segment.Length)
            {
                if (segment[currentIndex] == '=')
                {
                    string key = segment.Substring(startIndex, currentIndex - startIndex);
                    currentIndex++;
                    startIndex = currentIndex;

                    while (currentIndex <= segment.Length)
                    {
                        if (currentIndex == segment.Length || segment[currentIndex] == ',')
                        {
                            string value = segment.Substring(startIndex, currentIndex - startIndex);
                            key = key.Trim();
                            if (dictionary.ContainsKey(key))
                            {
                                throw new ODataException(Error.Format(SRResources.DuplicateKeyInSegment, key, segment));
                            }
                            dictionary.Add(key, value);
                            startIndex = currentIndex + 1;
                            break;
                        }

                        if (segment[currentIndex] == '\'')
                        {
                            currentIndex++;
                            while (currentIndex <= segment.Length)
                            {
                                if (currentIndex == segment.Length)
                                {
                                    throw new ODataException(
                                        Error.Format(SRResources.UnterminatedStringLiteral, startIndex, segment));
                                }

                                if (segment[currentIndex] == '\'')
                                {
                                    if (currentIndex + 1 == segment.Length)
                                    {
                                        break;
                                    }

                                    if (segment[currentIndex + 1] == '\'')
                                    {
                                        currentIndex++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }

                                currentIndex++;
                            }
                        }

                        currentIndex++;
                    }
                }

                currentIndex++;
            }

            // single key value.
            if (dictionary.Count == 0 && !String.IsNullOrWhiteSpace(segment))
            {
                dictionary.Add(String.Empty, segment);
            }

            return dictionary;
        }
    }
}
