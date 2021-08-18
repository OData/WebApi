//-----------------------------------------------------------------------------
// <copyright file="CookieHeaderValueInstanceCreator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

#if NETFX // This class is only used in the AspNet version.
using System;
using System.Collections.Specialized;
using System.Net.Http.Headers;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    public static class CookieHeaderValueInstanceCreator
    {
        public static readonly string CHAR;
        public static readonly string CTRL;
        public static readonly string Separators;
        public static readonly string Token;
        public static readonly string CookieOctet;
        public static readonly string Letter;
        public static readonly string Digit;
        public static readonly string Hyphen;
        public static readonly string PathValue;

        public static readonly Syntax DomainSyntax;

        static CookieHeaderValueInstanceCreator()
        {
            CHAR = new CharactorSet().Append((char)0, (char)127).ToString();
            CTRL = new CharactorSet().Append((char)0, (char)31).Append((char)127).ToString();
            Separators = new CharactorSet().Append("()<>@,;:\\\"/[]?={} ").Append((char)9).ToString();
            Token = new CharactorSet().Append((char)0, (char)127).Exclude(CTRL).Exclude(Separators).ToString();
            CookieOctet = new CharactorSet()
                .Append('\x21')
                .Append('\x23', '\x2B')
                .Append('\x2D', '\x3A')
                .Append('\x3C', '\x5B')
                .Append('\x5D', '\x7E')
                .ToString();
            Letter = new CharactorSet().Append('a', 'z').Append('A', 'Z').ToString();
            Digit = new CharactorSet().Append('0', '9').ToString();
            Hyphen = "-";
            PathValue = new CharactorSet().Append(CHAR).Exclude(CTRL).Exclude(';').ToString();

            var letter = new Token(Letter, 1, 1);
            var digit = new Token(Digit, 1, 1);
            var let_dig = letter | digit;
            var let_dig_hyp = let_dig | "-";
            var ldh_str = new NonTerminalSyntax();
            ldh_str.Syntax = let_dig_hyp | (let_dig_hyp + ldh_str);
            var label = letter + (ldh_str.OrEmpty() + let_dig).OrEmpty();
            var subdomain = new NonTerminalSyntax();
            subdomain.Syntax = label | (subdomain + "." + label);
            DomainSyntax = subdomain | " ";
        }

        public static CookieHeaderValue CreateInstanceOfCookieHeaderValue(Random rndGen, CreatorSettings creatorSettings)
        {
            creatorSettings.NullValueProbability = 0.0;
            string name = CreateRandomCookieName(rndGen, creatorSettings);

            CookieHeaderValue header;
            if (rndGen.Next(2) == 0)
            {
                header = new CookieHeaderValue(name, CreateRandomCookieValue(rndGen, creatorSettings));
            }
            else
            {
                header = new CookieHeaderValue(name, CreateRandomNameValueCollection(rndGen, creatorSettings));
            }

            header.Domain = CreateRandomDomain(rndGen, creatorSettings);
            header.Expires = InstanceCreator.CreateInstanceOf<DateTimeOffset?>(rndGen, creatorSettings);
            header.HttpOnly = InstanceCreator.CreateInstanceOf<bool>(rndGen, creatorSettings);
            header.MaxAge = InstanceCreator.CreateInstanceOf<TimeSpan?>(rndGen, creatorSettings);
            header.Secure = InstanceCreator.CreateInstanceOf<bool>(rndGen, creatorSettings);
            header.Path = CreateRandomPath(rndGen, creatorSettings);

            return header;
        }

        public static string CreateRandomCookieName(Random rndGen, CreatorSettings creatorSettings)
        {
            return PrimitiveCreator.CreateRandomString(rndGen, -1, Token, new CreatorSettings(creatorSettings) { MinStringLength = 1 });
        }

        public static string CreateRandomCookieValue(Random rndGen, CreatorSettings creatorSettings)
        {
            return PrimitiveCreator.CreateRandomString(rndGen, -1, CookieOctet, creatorSettings);
        }

        public static string CreateRandomDomain(Random rndGen, CreatorSettings creatorSettings)
        {
            return DomainSyntax.Generate(rndGen, creatorSettings);
        }

        public static string CreateRandomPath(Random rndGen, CreatorSettings creatorSettings)
        {
            return PrimitiveCreator.CreateRandomString(rndGen, -1, PathValue, creatorSettings);
        }

        private static NameValueCollection CreateRandomNameValueCollection(Random rndGen, CreatorSettings creatorSettings)
        {
            NameValueCollection col = new NameValueCollection();
            int size = rndGen.Next(20);
            for (int i = 0; i < size; i++)
            {
                col.Add(
                    CreateRandomCookieValue(rndGen, creatorSettings),
                    CreateRandomCookieValue(rndGen, creatorSettings));
            }

            return col;
        }
    }
}
#endif
