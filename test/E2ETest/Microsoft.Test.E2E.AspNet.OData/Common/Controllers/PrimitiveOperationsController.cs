//-----------------------------------------------------------------------------
// <copyright file="PrimitiveOperationsController.cs" company=".NET Foundation">
//      Copyright (c) .NET Foundation and Contributors. All rights reserved. 
//      See License.txt in the project root for license information.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Threading.Tasks;
using System.Web.Http;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers
{
    public class PrimitiveOperationsController : ApiController
    {
        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public int EchoInt32(int input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public short EchoInt16(short input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public long EchoInt64(long input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public uint EchoUInt32(uint input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public ushort EchoUInt16(ushort input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public ulong EchoUInt64(ulong input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public byte EchoByte(byte input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public sbyte EchoSByte(sbyte input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public double EchoDouble(double input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public float EchoSingle(float input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public decimal EchoDecimal(decimal input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public char EchoChar(char input)
        {
            return input;
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public bool EchoBoolean(bool input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public int EchoInt32FromBody([FromBody]int input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public short EchoInt16FromBody([FromBody]short input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public long EchoInt64FromBody([FromBody]long input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public uint EchoUInt32FromBody([FromBody]uint input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ushort EchoUInt16FromBody([FromBody]ushort input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public ulong EchoUInt64FromBody([FromBody]ulong input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public byte EchoByteFromBody([FromBody]byte input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public sbyte EchoSByteFromBody([FromBody]sbyte input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public double EchoDoubleFromBody([FromBody]double input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public float EchoSingleFromBody([FromBody]float input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public decimal EchoDecimalFromBody([FromBody]decimal input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public char EchoCharFromBody([FromBody]char input)
        {
            return input;
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public bool EchoBooleanFromBody([FromBody]bool input)
        {
            return input;
        }
    }

    public class PrimitiveOperationsAsyncController : ApiController
    {
        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<int> EchoInt32(int input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<short> EchoInt16(short input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<long> EchoInt64(long input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<uint> EchoUInt32(uint input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<ushort> EchoUInt16(ushort input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<ulong> EchoUInt64(ulong input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<byte> EchoByte(byte input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<sbyte> EchoSByte(sbyte input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<double> EchoDouble(double input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<float> EchoSingle(float input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<decimal> EchoDecimal(decimal input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<char> EchoChar(char input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("GET", "PUT", "POST", "DELETE")]
        public Task<bool> EchoBoolean(bool input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<int> EchoInt32FromBody([FromBody]int input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<short> EchoInt16FromBody([FromBody]short input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<long> EchoInt64FromBody([FromBody]long input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<uint> EchoUInt32FromBody([FromBody]uint input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<ushort> EchoUInt16FromBody([FromBody]ushort input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<ulong> EchoUInt64FromBody([FromBody]ulong input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<byte> EchoByteFromBody([FromBody]byte input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<sbyte> EchoSByteFromBody([FromBody]sbyte input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<double> EchoDoubleFromBody([FromBody]double input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<float> EchoSingleFromBody([FromBody]float input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<decimal> EchoDecimalFromBody([FromBody]decimal input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<char> EchoCharFromBody([FromBody]char input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<bool> EchoBooleanFromBody([FromBody]bool input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<string> EchoStringFromBody([FromBody]string input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<Guid> EchoGuidFromBody([FromBody]Guid input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<Uri> EchoUriFromBody([FromBody]Uri input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<TimeSpan> EchoTimeSpanFromBody([FromBody]TimeSpan input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<DBNull> EchoDBNullFromBody(DBNull input) // FromBody by default
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<DateTime> EchoDateTimeFromBody([FromBody] DateTime input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<DateTimeOffset> EchoDateTimeOffsetFromBody([FromBody] DateTimeOffset input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<int?> EchoNullableInt32FromBody([FromBody] int? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<short?> EchoNullableInt16FromBody([FromBody] short? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<long?> EchoNullableInt64FromBody([FromBody] long? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<uint?> EchoNullableUInt32FromBody([FromBody] uint? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<ushort?> EchoNullableUInt16FromBody([FromBody] ushort? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<ulong?> EchoNullableUInt64FromBody([FromBody] ulong? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<byte?> EchoNullableByteFromBody([FromBody] byte? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<sbyte?> EchoNullableSByteFromBody([FromBody] sbyte? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<double?> EchoNullableDoubleFromBody([FromBody] double? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<float?> EchoNullableSingleFromBody([FromBody] float? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<decimal?> EchoNullableDecimalFromBody([FromBody] decimal? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<char?> EchoNullableCharFromBody([FromBody] char? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<bool?> EchoNullableBooleanFromBody([FromBody] bool? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<Guid?> EchoNullableGuidFromBody([FromBody]Guid? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<TimeSpan?> EchoNullableTimeSpanFromBody([FromBody]TimeSpan? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<DateTime?> EchoNullableDateTimeFromBody([FromBody] DateTime? input)
        {
            return Task.Factory.StartNew(() => input);
        }

        [AcceptVerbs("PUT", "POST", "DELETE")]
        public Task<DateTimeOffset?> EchoNullableDateTimeOffsetFromBody([FromBody] DateTimeOffset? input)
        {
            return Task.Factory.StartNew(() => input);
        }
    }
}
