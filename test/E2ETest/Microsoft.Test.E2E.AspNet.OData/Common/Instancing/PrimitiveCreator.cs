//-----------------------------------------------------------------------------
// <copyright file="PrimitiveCreator.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Instancing
{
    public static class PrimitiveCreator
    {
        #region Constants and Fields

        public const string AllLetters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public const string AllLettersAndNumbers = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        private static readonly Dictionary<Type, MethodInfo> creators;

        #endregion

        #region Constructors and Destructors

        static PrimitiveCreator()
        {
            Type primitiveCreatorType = typeof(PrimitiveCreator);
            creators = new Dictionary<Type, MethodInfo>();
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Static;
            creators.Add(typeof(Boolean), primitiveCreatorType.GetMethod("CreateInstanceOfBoolean", bindingFlags));
            creators.Add(typeof(Byte), primitiveCreatorType.GetMethod("CreateInstanceOfByte", bindingFlags));
            creators.Add(typeof(Char), primitiveCreatorType.GetMethod("CreateInstanceOfChar", bindingFlags));
            creators.Add(typeof(DateTime), primitiveCreatorType.GetMethod("CreateInstanceOfDateTime", bindingFlags));
            creators.Add(
                typeof(DateTimeOffset), primitiveCreatorType.GetMethod("CreateInstanceOfDateTimeOffset", bindingFlags));
            creators.Add(typeof(DBNull), primitiveCreatorType.GetMethod("CreateInstanceOfDBNull", bindingFlags));
            creators.Add(typeof(Decimal), primitiveCreatorType.GetMethod("CreateInstanceOfDecimal", bindingFlags));
            creators.Add(typeof(Double), primitiveCreatorType.GetMethod("CreateInstanceOfDouble", bindingFlags));
            creators.Add(typeof(Guid), primitiveCreatorType.GetMethod("CreateInstanceOfGuid", bindingFlags));
            creators.Add(typeof(Int16), primitiveCreatorType.GetMethod("CreateInstanceOfInt16", bindingFlags));
            creators.Add(typeof(Int32), primitiveCreatorType.GetMethod("CreateInstanceOfInt32", bindingFlags));
            creators.Add(typeof(Int64), primitiveCreatorType.GetMethod("CreateInstanceOfInt64", bindingFlags));
            creators.Add(typeof(Object), primitiveCreatorType.GetMethod("CreateInstanceOfObject", bindingFlags));
            creators.Add(typeof(SByte), primitiveCreatorType.GetMethod("CreateInstanceOfSByte", bindingFlags));
            creators.Add(typeof(Single), primitiveCreatorType.GetMethod("CreateInstanceOfSingle", bindingFlags));
            creators.Add(
                typeof(String),
                primitiveCreatorType.GetMethod(
                    "CreateInstanceOfString",
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    new Type[] { typeof(Random), typeof(CreatorSettings) },
                    null));
            creators.Add(typeof(TimeSpan), primitiveCreatorType.GetMethod("CreateInstanceOfTimeSpan", bindingFlags));
            creators.Add(typeof(UInt16), primitiveCreatorType.GetMethod("CreateInstanceOfUInt16", bindingFlags));
            creators.Add(typeof(UInt32), primitiveCreatorType.GetMethod("CreateInstanceOfUInt32", bindingFlags));
            creators.Add(typeof(UInt64), primitiveCreatorType.GetMethod("CreateInstanceOfUInt64", bindingFlags));
            creators.Add(typeof(Uri), primitiveCreatorType.GetMethod("CreateInstanceOfUri", bindingFlags));
        }

        #endregion

        #region Public Methods and Operators

        public static bool CanCreateInstanceOf(Type type)
        {
            return creators.ContainsKey(type);
        }

        public static Boolean CreateInstanceOfBoolean(Random rndGen, CreatorSettings creatorSettings)
        {
            return rndGen.Next(2) == 0;
        }

        public static Byte CreateInstanceOfByte(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] rndValue = new byte[1];
            rndGen.NextBytes(rndValue);
            return rndValue[0];
        }

        public static Char CreateInstanceOfChar(Random rndGen, CreatorSettings creatorSettings)
        {
            if (creatorSettings.CreateOnlyAsciiChars)
            {
                return (Char)rndGen.Next(0x20, 0x7F);
            }
            else if (creatorSettings.DontCreateSurrogateChars)
            {
                char c;
                do
                {
                    c = (Char)rndGen.Next((int)Char.MinValue, (int)Char.MaxValue);
                }
                while (Char.IsSurrogate(c));
                return c;
            }
            else
            {
                return (Char)rndGen.Next((int)Char.MinValue, (int)Char.MaxValue + 1);
            }
        }

        public static DBNull CreateInstanceOfDBNull(Random rndGen, CreatorSettings creatorSettings)
        {
            return (rndGen.Next(2) == 0) ? null : DBNull.Value;
        }

        public static DateTime CreateInstanceOfDateTime(Random rndGen, CreatorSettings creatorSettings)
        {
            long temp = CreateInstanceOfInt64(rndGen, creatorSettings);
            temp = Math.Abs(temp);
            DateTime result;
            try
            {
                result = new DateTime(temp % (DateTime.MaxValue.Ticks + 1));
            }
            catch (ArgumentOutOfRangeException)
            {
                // jasonv - approved; specific, commented
                // From http://msdn.microsoft.com/en-us/library/z2xf7zzk.aspx
                // ticks is less than MinValue or greater than MaxValue. 
                result = DateTime.Now;
            }

            int kind = rndGen.Next(3);
            switch (kind)
            {
                case 0:
                    result = DateTime.SpecifyKind(result, DateTimeKind.Local);
                    break;
                case 1:
                    result = DateTime.SpecifyKind(result, DateTimeKind.Unspecified);
                    break;
                default:
                    result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
                    break;
            }

            if (!creatorSettings.CreateDateTimeWithSubMilliseconds)
            {
                result = new DateTime(
                    result.Year,
                    result.Month,
                    result.Day,
                    result.Hour,
                    result.Minute,
                    result.Second,
                    result.Millisecond,
                    result.Kind);
            }

            return result;
        }

        public static DateTimeOffset CreateInstanceOfDateTimeOffset(Random rndGen, CreatorSettings creatorSettings)
        {
            DateTime temp = CreateInstanceOfDateTime(rndGen, creatorSettings);
            temp = DateTime.SpecifyKind(temp, DateTimeKind.Unspecified);
            int offsetMinutes = rndGen.Next(-14 * 60, 14 * 60);
            DateTimeOffset result = new DateTimeOffset(temp, TimeSpan.FromMinutes(offsetMinutes));
            return result;
        }

        public static Decimal CreateInstanceOfDecimal(Random rndGen, CreatorSettings creatorSettings)
        {
            int low = CreateInstanceOfInt32(rndGen, creatorSettings);
            int mid = CreateInstanceOfInt32(rndGen, creatorSettings);
            int high = CreateInstanceOfInt32(rndGen, creatorSettings);
            bool isNegative = rndGen.Next(2) == 0;
            const int MaxDecimalScale = 28;
            byte scale = (byte)rndGen.Next(0, MaxDecimalScale + 1);
            return new Decimal(low, mid, high, isNegative, scale);
        }

        public static Double CreateInstanceOfDouble(Random rndGen, CreatorSettings creatorSettings)
        {
            bool negative = rndGen.Next(2) == 0;
            int temp = rndGen.Next(40);
            Double result;
            switch (temp)
            {
                case 0:
                    return Double.NaN;
                case 1:
                    return Double.PositiveInfinity;
                case 2:
                    return Double.NegativeInfinity;
                case 3:
                    return Double.MinValue;
                case 4:
                    return Double.MaxValue;
                case 5:
                    return Double.Epsilon;
                default:
                    result = (Double)(rndGen.NextDouble() * 100000);
                    if (negative)
                    {
                        result = -result;
                    }

                    return result;
            }
        }

        public static Guid CreateInstanceOfGuid(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] temp = new byte[16];
            rndGen.NextBytes(temp);
            return new Guid(temp);
        }

        public static Int16 CreateInstanceOfInt16(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] rndValue = new byte[2];
            rndGen.NextBytes(rndValue);
            Int16 result = 0;
            for (int i = 0; i < rndValue.Length; i++)
            {
                result = (Int16)(result << 8);
                result = (Int16)(result | (Int16)rndValue[i]);
            }

            return result;
        }

        public static Int32 CreateInstanceOfInt32(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] rndValue = new byte[4];
            rndGen.NextBytes(rndValue);
            Int32 result = 0;
            for (int i = 0; i < rndValue.Length; i++)
            {
                result = (Int32)(result << 8);
                result = (Int32)(result | (Int32)rndValue[i]);
            }

            return result;
        }

        public static Int64 CreateInstanceOfInt64(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] rndValue = new byte[8];
            rndGen.NextBytes(rndValue);
            Int64 result = 0;
            for (int i = 0; i < rndValue.Length; i++)
            {
                result = (Int64)(result << 8);
                result = (Int64)(result | (Int64)rndValue[i]);
            }

            return result;
        }

        public static Object CreateInstanceOfObject(Random rndGen, CreatorSettings creatorSettings)
        {
            return (rndGen.Next(5) == 0) ? null : new Object();
        }

        //[CLSCompliant(false)]
        public static SByte CreateInstanceOfSByte(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] rndValue = new byte[1];
            rndGen.NextBytes(rndValue);
            SByte result = (SByte)rndValue[0];
            return result;
        }

        public static Single CreateInstanceOfSingle(Random rndGen, CreatorSettings creatorSettings)
        {
            bool negative = rndGen.Next(2) == 0;
            int temp = rndGen.Next(40);
            Single result;
            switch (temp)
            {
                case 0:
                    return Single.NaN;
                case 1:
                    return Single.PositiveInfinity;
                case 2:
                    return Single.NegativeInfinity;
                case 3:
                    return Single.MinValue;
                case 4:
                    return Single.MaxValue;
                case 5:
                    return Single.Epsilon;
                default:
                    result = (Single)(rndGen.NextDouble() * 100000);
                    if (negative)
                    {
                        result = -result;
                    }

                    return result;
            }
        }

        public static string CreateInstanceOfString(Random rndGen, CreatorSettings creatorSettings)
        {
            double rndNumber = rndGen.NextDouble();
            if (rndNumber < creatorSettings.NullValueProbability)
            {
                return null;
            }

            return CreateRandomString(rndGen, -1, null, creatorSettings);
        }

        public static string CreateInstanceOfString(Random rndGen, int size, string charsToUse)
        {
            return CreateRandomString(rndGen, size, charsToUse, new CreatorSettings());
        }

        public static TimeSpan CreateInstanceOfTimeSpan(Random rndGen, CreatorSettings creatorSettings)
        {
            long temp = CreateInstanceOfInt64(rndGen, creatorSettings);
            TimeSpan result = TimeSpan.FromTicks(temp);
            return result;
        }

        //[CLSCompliant(false)]
        public static UInt16 CreateInstanceOfUInt16(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] rndValue = new byte[2];
            rndGen.NextBytes(rndValue);
            UInt16 result = 0;
            for (int i = 0; i < rndValue.Length; i++)
            {
                result = (UInt16)(result << 8);
                result = (UInt16)(result | (UInt16)rndValue[i]);
            }

            return result;
        }

        //[CLSCompliant(false)]
        public static UInt32 CreateInstanceOfUInt32(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] rndValue = new byte[4];
            rndGen.NextBytes(rndValue);
            UInt32 result = 0;
            for (int i = 0; i < rndValue.Length; i++)
            {
                result = (UInt32)(result << 8);
                result = (UInt32)(result | (UInt32)rndValue[i]);
            }

            return result;
        }

        //[CLSCompliant(false)]
        public static UInt64 CreateInstanceOfUInt64(Random rndGen, CreatorSettings creatorSettings)
        {
            byte[] rndValue = new byte[8];
            rndGen.NextBytes(rndValue);
            UInt64 result = 0;
            for (int i = 0; i < rndValue.Length; i++)
            {
                result = (UInt64)(result << 8);
                result = (UInt64)(result | (UInt64)rndValue[i]);
            }

            return result;
        }

        public static Uri CreateInstanceOfUri(Random rndGen, CreatorSettings creatorSettings)
        {
            Uri result = null;
            UriKind kind;
            try
            {
                string uriString = UriCreator.CreateUri(rndGen, out kind);
                result = new Uri(uriString, kind);
            }
            catch (ArgumentException)
            {
                // From http://msdn.microsoft.com/en-us/library/ms131565.aspx
                // uriKind is invalid.
                result = new Uri("my.schema://userName:password@my.domain/path1/path2?query1=123&query2=%22hello%22");
            }
            catch (UriFormatException)
            {
                // From http://msdn.microsoft.com/en-us/library/ms131565.aspx
                // uriKind is invalid.
                result = new Uri("my.schema://userName:password@my.domain/path1/path2?query1=123&query2=%22hello%22");
            }

            return result;
        }

        public static object CreatePrimitiveInstance(Type type, Random rndGen, CreatorSettings creatorSettings)
        {
            if (creators.ContainsKey(type))
            {
                return creators[type].Invoke(null, new object[] { rndGen, creatorSettings });
            }
            else
            {
                throw new ArgumentException("Type " + type.FullName + " not supported");
            }
        }

        #endregion

        #region Methods

        public static string CreateRandomString(
            Random rndGen, int size, string charsToUse, CreatorSettings creatorSettings)
        {
            int maxSize = creatorSettings.MaxStringLength;
            int minSize = creatorSettings.MinStringLength;
            // invalid per the XML spec (http://www.w3.org/TR/REC-xml/#charsets), cannot be sent as XML
            const string InvalidXmlChars =
                "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\u0008\u000B\u000C\u000E\u000F\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001A\u001B\u001C\u001D\u001E\u001F\uFFFE\uFFFF";
            const int LowSurrogateMin = 0xDC00;
            const int LowSurrogateMax = 0xDFFF;
            const int HighSurrogateMin = 0xD800;
            const int HighSurrogateMax = 0xDBFF;

            if (size < 0)
            {
                double rndNumber = rndGen.NextDouble();
                if (rndNumber < creatorSettings.NullValueProbability)
                {
                    return null; // 1% chance of null value
                }

                if (maxSize > minSize)
                {
                    size = (int)Math.Pow(maxSize - minSize, rndNumber); // this will create more small strings than large ones
                    size += minSize;
                }
                else
                {
                    // if minsize equals to maxsize
                    size = maxSize;
                }
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < size; i++)
            {
                char c;
                if (charsToUse != null)
                {
                    c = charsToUse[rndGen.Next(charsToUse.Length)];
                    sb.Append(c);
                }
                else
                {
                    if (creatorSettings.CreateOnlyAsciiChars || rndGen.Next(2) == 0)
                    {
                        c = (char)rndGen.Next(0x20, 0x7F); // low-ascii chars
                        sb.Append(c);
                    }
                    else
                    {
                        do
                        {
                            c = (char)rndGen.Next((int)char.MinValue, (int)char.MaxValue + 1);
                        }
                        while ((LowSurrogateMin <= c && c <= LowSurrogateMax) || (InvalidXmlChars.IndexOf(c) >= 0));
                        sb.Append(c);
                        if (HighSurrogateMin <= c && c <= HighSurrogateMax)
                        {
                            // need to add a low surrogate
                            c = (char)rndGen.Next(LowSurrogateMin, LowSurrogateMax + 1);
                            sb.Append(c);
                        }
                    }
                }
            }

            return sb.ToString();
        }

        #endregion

        /// <summary>
        /// Creates URI instances based on RFC 2396
        /// </summary>
        internal static class UriCreator
        {
            #region Constants and Fields

            private static readonly string alpha;

            private static readonly string alphanum;

            private static readonly string digit;

            private static readonly string hex;

            private static readonly string lowalpha;

            private static readonly string mark;

            private static readonly string reserved;

            private static readonly string unreserved;

            private static readonly string upalpha;

            #endregion

            #region Constructors and Destructors

            static UriCreator()
            {
                digit = "0123456789";
                upalpha = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
                lowalpha = upalpha.ToLower();
                alpha = upalpha + lowalpha;
                alphanum = alpha + digit;
                hex = digit + "ABCDEFabcdef";
                mark = "-_.!~*'()";
                unreserved = alphanum + mark;
                reserved = ";/?:@&=+$,";
            }

            #endregion

            #region Methods

            internal static string CreateUri(Random rndGen, out UriKind kind)
            {
                StringBuilder sb = new StringBuilder();
                //kind = UriKind.Relative;
                //Devdiv bug 187103
                kind = UriKind.Absolute;
                if (rndGen.Next(3) > 0)
                {
                    // Add URI scheme
                    CreateScheme(sb, rndGen);
                    kind = UriKind.Absolute;
                }

                if (rndGen.Next(3) > 0)
                {
                    // Add URI host
                    sb.Append("//");
                    if (rndGen.Next(10) == 0)
                    {
                        CreateUserInfo(sb, rndGen);
                    }

                    CreateHost(sb, rndGen);
                    if (rndGen.Next(2) > 0)
                    {
                        sb.Append(':');
                        sb.Append(rndGen.Next(65536));
                    }
                }

                if (rndGen.Next(4) > 0)
                {
                    // Add URI path
                    for (int i = 0; i < rndGen.Next(1, 4); i++)
                    {
                        sb.Append('/');
                        AddPathSegment(sb, rndGen);
                    }
                }

                if (rndGen.Next(3) == 0)
                {
                    // Add URI query string
                    sb.Append('?');
                    AddUriC(sb, rndGen);
                }

                return sb.ToString();
            }

            private static void AddChars(StringBuilder sb, Random rndGen, string validChars, int size)
            {
                for (int i = 0; i < size; i++)
                {
                    sb.Append(validChars[rndGen.Next(validChars.Length)]);
                }
            }

            private static void AddEscapedChar(StringBuilder sb, Random rndGen)
            {
                sb.Append('%');
                AddChars(sb, rndGen, hex, 2);
            }

            private static void AddIPv6AddressPart(StringBuilder sb, Random rndGen)
            {
                int size = rndGen.Next(1, 10);
                if (size > 4)
                {
                    size = 4;
                }

                AddChars(sb, rndGen, hex, size);
            }

            private static void AddPathSegment(StringBuilder sb, Random rndGen)
            {
                string pchar = unreserved + ":@&=+$,";
                int size = rndGen.Next(1, 10);
                for (int i = 0; i < size; i++)
                {
                    if (rndGen.Next(pchar.Length + 1) > 0)
                    {
                        AddChars(sb, rndGen, pchar, 1);
                    }
                    else
                    {
                        AddEscapedChar(sb, rndGen);
                    }
                }
            }

            private static void AddUriC(StringBuilder sb, Random rndGen)
            {
                int size = rndGen.Next(20);
                string reservedPlusUnreserved = reserved + unreserved;
                for (int i = 0; i < size; i++)
                {
                    if (rndGen.Next(5) > 0)
                    {
                        AddChars(sb, rndGen, reservedPlusUnreserved, 1);
                    }
                    else
                    {
                        AddEscapedChar(sb, rndGen);
                    }
                }
            }

            private static void CreateHost(StringBuilder sb, Random rndGen)
            {
                int temp = rndGen.Next(3);
                switch (temp)
                {
                    case 0:
                        CreateIPv4Address(sb, rndGen);
                        break;
                    case 1:
                        CreateIPv6Address(sb, rndGen);
                        break;
                    case 2:
                        CreateHostName(sb, rndGen);
                        break;
                }
            }

            private static void CreateHostName(StringBuilder sb, Random rndGen)
            {
                int domainLabelCount = rndGen.Next(4);
                int size;
                for (int i = 0; i < domainLabelCount; i++)
                {
                    AddChars(sb, rndGen, alphanum, 1);
                    size = rndGen.Next(10) - 1;
                    if (size > 0)
                    {
                        AddChars(sb, rndGen, alphanum + "-", size);
                        AddChars(sb, rndGen, alphanum, 1);
                    }

                    sb.Append('.');
                }

                AddChars(sb, rndGen, alpha, 1);
                size = rndGen.Next(10) - 1;
                if (size > 0)
                {
                    AddChars(sb, rndGen, alphanum + "-", size);
                    AddChars(sb, rndGen, alphanum, 1);
                }
            }

            private static void CreateIPv4Address(StringBuilder sb, Random rndGen)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (i > 0)
                    {
                        sb.Append('.');
                    }

                    sb.Append(rndGen.Next(1000));
                }
            }

            private static void CreateIPv6Address(StringBuilder sb, Random rndGen)
            {
                sb.Append('[');
                int temp = rndGen.Next(6);
                int i;
                switch (temp)
                {
                    case 0:
                        sb.Append("::");
                        break;
                    case 1:
                        sb.Append("::1");
                        break;
                    case 2:
                        sb.Append("FF01::101");
                        break;
                    case 3:
                        sb.Append("::1");
                        break;
                    case 4:
                        for (i = 0; i < 3; i++)
                        {
                            AddIPv6AddressPart(sb, rndGen);
                            sb.Append(':');
                        }

                        for (i = 0; i < 3; i++)
                        {
                            sb.Append(':');
                            AddIPv6AddressPart(sb, rndGen);
                        }

                        break;
                    default:
                        for (i = 0; i < 8; i++)
                        {
                            if (i > 0)
                            {
                                sb.Append(':');
                            }

                            AddIPv6AddressPart(sb, rndGen);
                        }

                        break;
                }

                sb.Append(']');
            }

            private static void CreateScheme(StringBuilder sb, Random rndGen)
            {
                int size = rndGen.Next(1, 10);
                AddChars(sb, rndGen, alpha, 1);
                string schemeChars = alpha + digit + "+-.";
                AddChars(sb, rndGen, schemeChars, size);
                sb.Append(':');
            }

            private static void CreateUserInfo(StringBuilder sb, Random rndGen)
            {
                AddChars(sb, rndGen, alpha, rndGen.Next(1, 10));
                if (rndGen.Next(3) > 0)
                {
                    sb.Append(':');
                    AddChars(sb, rndGen, alpha, rndGen.Next(1, 10));
                }

                sb.Append('@');
            }

            #endregion
        }
    }
}
