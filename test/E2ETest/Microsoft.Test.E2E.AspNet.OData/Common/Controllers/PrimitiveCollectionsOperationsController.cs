//-----------------------------------------------------------------------------
// <copyright file="PrimitiveCollectionsOperationsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    public class PrimitiveCollectionsOperationsController : ApiController
    {
        #region Reverse*Array([FromUri]...)
        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public bool[] ReverseBooleanArray([FromUri] bool[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public byte[] ReverseByteArray([FromUri]byte[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public char[] ReverseCharArray([FromUri]char[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public decimal[] ReverseDecimalArray([FromUri]decimal[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public double[] ReverseDoubleArray([FromUri]double[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public short[] ReverseInt16Array([FromUri]short[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public int[] ReverseInt32Array([FromUri]int[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public long[] ReverseInt64Array([FromUri]long[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public sbyte[] ReverseSByteArray([FromUri]sbyte[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public float[] ReverseSingleArray([FromUri]float[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public ushort[] ReverseUInt16Array([FromUri]ushort[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public uint[] ReverseUInt32Array([FromUri]uint[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public ulong[] ReverseUInt64Array([FromUri]ulong[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }
        #endregion

        #region Reverse*ArrayFromBody(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public bool[] ReverseBooleanArrayFromBody(bool[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public byte[] ReverseByteArrayFromBody(byte[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public char[] ReverseCharArrayFromBody(char[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public decimal[] ReverseDecimalArrayFromBody(decimal[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public double[] ReverseDoubleArrayFromBody(double[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public short[] ReverseInt16ArrayFromBody(short[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public int[] ReverseInt32ArrayFromBody(int[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public long[] ReverseInt64ArrayFromBody(long[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public sbyte[] ReverseSByteArrayFromBody(sbyte[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public float[] ReverseSingleArrayFromBody(float[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ushort[] ReverseUInt16ArrayFromBody(ushort[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public uint[] ReverseUInt32ArrayFromBody(uint[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ulong[] ReverseUInt64ArrayFromBody(ulong[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public string[] ReverseStringArrayFromBody(string[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Guid[] ReverseGuidArrayFromBody(Guid[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Uri[] ReverseUriArrayFromBody(Uri[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public TimeSpan[] ReverseTimeSpanArrayFromBody(TimeSpan[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DateTime[] ReverseDateTimeArrayFromBody(DateTime[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DateTimeOffset[] ReverseDateTimeOffsetArrayFromBody(DateTimeOffset[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DBNull[] ReverseDBNullArrayFromBody(DBNull[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }
        #endregion

        #region ReverseNullable*ArrayFromBody(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public bool?[] ReverseNullableBooleanArrayFromBody(bool?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public byte?[] ReverseNullableByteArrayFromBody(byte?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public char?[] ReverseNullableCharArrayFromBody(char?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public decimal?[] ReverseNullableDecimalArrayFromBody(decimal?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public double?[] ReverseNullableDoubleArrayFromBody(double?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public short?[] ReverseNullableInt16ArrayFromBody(short?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public int?[] ReverseNullableInt32ArrayFromBody(int?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public long?[] ReverseNullableInt64ArrayFromBody(long?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public sbyte?[] ReverseNullableSByteArrayFromBody(sbyte?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public float?[] ReverseNullableSingleArrayFromBody(float?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ushort?[] ReverseNullableUInt16ArrayFromBody(ushort?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public uint?[] ReverseNullableUInt32ArrayFromBody(uint?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ulong?[] ReverseNullableUInt64ArrayFromBody(ulong?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Guid?[] ReverseNullableGuidArrayFromBody(Guid?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public TimeSpan?[] ReverseNullableTimeSpanArrayFromBody(TimeSpan?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DateTime?[] ReverseNullableDateTimeArrayFromBody(DateTime?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public DateTimeOffset?[] ReverseNullableDateTimeOffsetArrayFromBody(DateTimeOffset?[] input)
        {
            return (input == null) ? null : input.Reverse().ToArray();
        }
        #endregion

        #region ReverseListOf*FromBody(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<int> ReverseListOfInt32FromBody(List<int> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<short> ReverseListOfInt16FromBody(List<short> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<long> ReverseListOfInt64FromBody(List<long> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<uint> ReverseListOfUInt32FromBody(List<uint> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<ushort> ReverseListOfUInt16FromBody(List<ushort> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<ulong> ReverseListOfUInt64FromBody(List<ulong> input)
        {
            input.Reverse();
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<byte> ReverseListOfByteFromBody(List<byte> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<sbyte> ReverseListOfSByteFromBody(List<sbyte> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<double> ReverseListOfDoubleFromBody(List<double> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<float> ReverseListOfSingleFromBody(List<float> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<decimal> ReverseListOfDecimalFromBody(List<decimal> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<char> ReverseListOfCharFromBody(List<char> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<bool> ReverseListOfBooleanFromBody(List<bool> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<string> ReverseListOfStringFromBody(List<string> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<Guid> ReverseListOfGuidFromBody(List<Guid> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<Uri> ReverseListOfUriFromBody(List<Uri> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<TimeSpan> ReverseListOfTimeSpanFromBody(List<TimeSpan> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DateTime> ReverseListOfDateTimeFromBody(List<DateTime> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DateTimeOffset> ReverseListOfDateTimeOffsetFromBody(List<DateTimeOffset> input)
        {
            input.Reverse();
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DBNull> ReverseListOfDBNullFromBody(List<DBNull> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }
        #endregion

        #region ReverseListOfNullable*FromBody(...)
        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<int?> ReverseListOfNullableInt32FromBody(List<int?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<short?> ReverseListOfNullableInt16FromBody(List<short?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<long?> ReverseListOfNullableInt64FromBody(List<long?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<uint?> ReverseListOfNullableUInt32FromBody(List<uint?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<ushort?> ReverseListOfNullableUInt16FromBody(List<ushort?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<ulong?> ReverseListOfNullableUInt64FromBody(List<ulong?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<byte?> ReverseListOfNullableByteFromBody(List<byte?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<sbyte?> ReverseListOfNullableSByteFromBody(List<sbyte?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<double?> ReverseListOfNullableDoubleFromBody(List<double?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<float?> ReverseListOfNullableSingleFromBody(List<float?> input)
        {
            input.Reverse();
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<decimal?> ReverseListOfNullableDecimalFromBody(List<decimal?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<char?> ReverseListOfNullableCharFromBody(List<char?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<bool?> ReverseListOfNullableBooleanFromBody(List<bool?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<Guid?> ReverseListOfNullableGuidFromBody(List<Guid?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<TimeSpan?> ReverseListOfNullableTimeSpanFromBody(List<TimeSpan?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DateTime?> ReverseListOfNullableDateTimeFromBody(List<DateTime?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public List<DateTimeOffset?> ReverseListOfNullableDateTimeOffsetFromBody(List<DateTimeOffset?> input)
        {
            if (input != null)
            {
                input.Reverse();
            }
            return input;
        }
        #endregion
    }
}
