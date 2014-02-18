// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Http;
using System.Web.OData.Properties;
using Microsoft.OData.Core;

namespace System.Web.OData.Routing
{
    /// <summary>
    /// Parses the OData multi-key value (same for function call parameters) as a dictionary.
    /// </summary>
    internal static class KeyValueParser
    {
        private static readonly Regex _stringLiteralRegex = new Regex(@"^'([^']|'')*'$", RegexOptions.Compiled);

        // TODO 1656: Make this method support more format in OData Uri BNF
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

                    if (String.IsNullOrWhiteSpace(key))
                    {
                        throw new ODataException(
                            Error.Format(SRResources.NoKeyNameFoundInSegment, startIndex, segment));
                    }

                    // Simple key which contains '='.
                    if (key.Contains("'"))
                    {
                        if (dictionary.Count != 0)
                        {
                            throw new ODataException(
                                Error.Format(SRResources.NoKeyNameFoundInSegment, startIndex, segment));
                        }

                        CheckSingleQuote(segment, segment);
                        dictionary.Add(String.Empty, segment);
                        return dictionary;
                    }

                    currentIndex++;
                    startIndex = currentIndex;

                    while (currentIndex <= segment.Length)
                    {
                        if (currentIndex == segment.Length || segment[currentIndex] == ',')
                        {
                            string value = segment.Substring(startIndex, currentIndex - startIndex);

                            if (String.IsNullOrWhiteSpace(value))
                            {
                                throw new ODataException(
                                    Error.Format(SRResources.NoValueLiteralFoundInSegment, key, startIndex, segment));
                            }

                            if (dictionary.ContainsKey(key))
                            {
                                throw new ODataException(Error.Format(SRResources.DuplicateKeyInSegment, key, segment));
                            }

                            CheckSingleQuote(value, segment);
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

            // Simple key.
            if (dictionary.Count == 0 && !String.IsNullOrWhiteSpace(segment))
            {
                CheckSingleQuote(segment, segment);
                dictionary.Add(String.Empty, segment);
            }

            return dictionary;
        }

        private static void CheckSingleQuote(string value, string segment)
        {
            if (value.StartsWith("'", StringComparison.Ordinal))
            {
                // String literal
                if (!_stringLiteralRegex.IsMatch(value))
                {
                    throw new ODataException(
                        Error.Format(SRResources.LiteralHasABadFormat, value, segment));
                }
            }
            else
            {
                int singleQuoteCount = value.Count(c => c == '\'');

                if (singleQuoteCount != 0 && singleQuoteCount != 2)
                {
                    throw new ODataException(
                        Error.Format(SRResources.InvalidSingleQuoteCountForNonStringLiteral, value, segment));
                }

                if (singleQuoteCount != 0 && !value.EndsWith("'", StringComparison.Ordinal))
                {
                    throw new ODataException(
                        Error.Format(SRResources.LiteralHasABadFormat, value, segment));
                }
            }
        }
    }
}
