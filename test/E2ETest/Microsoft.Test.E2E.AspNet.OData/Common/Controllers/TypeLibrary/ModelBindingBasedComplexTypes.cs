// Copyright (c) Microsoft Corporation.  All rights reserved.
// Licensed under the MIT License.  See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.Test.E2E.AspNet.OData.Common.Instancing;

namespace Microsoft.Test.E2E.AspNet.OData.Common.Controllers.TypeLibrary.ModelBindingBasedTypes
{
    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public enum EnumType_17
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_0,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_1 = 2,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_2,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_3,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_4,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_5,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_6,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_7,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_8 = 9,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_9,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_10,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_11 = 12,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_12 = 13,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_13,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_14,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_15,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_16,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_17,
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public enum EnumType_35
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_0,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_1,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_2,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_3,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_4,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_5,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_6,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_7,

        /// <summary>
        /// Test member.
        /// </summary>
        [EnumMember]
        member_8,
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public interface IEmptyInterface
    {
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public struct StructInt16
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public short Int16Member { get; set; }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public struct StructGuid
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public Guid GuidMember { get; set; }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_1
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public byte Member0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_1 other = obj as DCType_1;
            return (other != null) && this.Member0.Equals(other.Member0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = this.Member0.GetHashCode();
            return result;
        }

        /// <summary>
        /// Returns a debug representation for this instance.
        /// </summary>
        /// <returns>A debug representation for this instance.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "DCType_1<Member0={0:X2}>", (int)this.Member0);
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_3
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        // [CLSCompliant(false)]
        public ulong Member2 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_3 other = obj as DCType_3;
            return (other != null) && this.Member2.Equals(other.Member2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = this.Member2.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class SerType_4
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public char Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public short? Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            SerType_4 other = obj as SerType_4;
            return (other != null) && this.Member0.Equals(other.Member0) && ComparisonHelper.CompareNullable<short>(this.Member1, other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Member0.GetHashCode();
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            return result;
        }

        /// <summary>
        /// Returns a debug representation for this instance.
        /// </summary>
        /// <returns>A debug representation for this instance.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "SerType_4<Member0=(char){0},Member1={1}>", (int)this.Member0, ComparisonHelper.EscapeString(this.Member1));
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class SerType_5
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public char Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public byte? Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public char Member2 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public bool Member3 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        // [CLSCompliant(false)]
        public sbyte Member4 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            SerType_5 other = obj as SerType_5;
            return (other != null) &&
                this.Member0.Equals(other.Member0) &&
                ComparisonHelper.CompareNullable<byte>(this.Member1, other.Member1) &&
                this.Member2.Equals(other.Member2) &&
                this.Member3.Equals(other.Member3) &&
                this.Member4.Equals(other.Member4);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Member0.GetHashCode();
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            result ^= this.Member2.GetHashCode();
            result ^= this.Member3.GetHashCode();
            result ^= this.Member4.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_7
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public long? Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        // [CLSCompliant(false)]
        public sbyte Member2 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public int[] Member3 { get; set; }
        //Bug 389440 - public byte[] Member3 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_7 other = obj as DCType_7;
            return (other != null) &&
                ComparisonHelper.CompareNullable<long>(this.Member1, other.Member1) &&
                this.Member2.Equals(other.Member2) &&
                ComparisonHelper.CompareArrays(this.Member3, other.Member3);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            result ^= this.Member2.GetHashCode();
            result ^= ComparisonHelper.ComputeArrayHashCode(this.Member3);
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_9
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        // [CLSCompliant(false)]
        public sbyte? Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public Guid Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_9 other = obj as DCType_9;
            return (other != null) &&
                ComparisonHelper.CompareNullable<sbyte>(this.Member0, other.Member0) &&
                this.Member1.Equals(other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= this.Member1.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class SerType_11
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public float Member0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            SerType_11 other = obj as SerType_11;
            return (other != null) && this.Member0.Equals(other.Member0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = this.Member0.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_15
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public int[] Member0 { get; set; }
        //Bug 389440 - public byte[] Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        // [CLSCompliant(false)]
        [DataMember]
        public ushort Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public Guid Member2 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_1 Member3 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_7 Member5 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public int Member6 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_9 Member7 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_15 other = obj as DCType_15;
            return (other != null) &&
                ComparisonHelper.CompareArrays(this.Member0, other.Member0) &&
                this.Member1.Equals(other.Member1) &&
                this.Member2.Equals(other.Member2) &&
                ComparisonHelper.CompareObjects<DCType_1>(this.Member3, other.Member3) &&
                ComparisonHelper.CompareObjects<DCType_7>(this.Member5, other.Member5) &&
                this.Member6.Equals(other.Member6) &&
                ComparisonHelper.CompareObjects<DCType_9>(this.Member7, other.Member7);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= ComparisonHelper.ComputeArrayHashCode(this.Member0);
            result ^= this.Member1.GetHashCode();
            result ^= this.Member2.GetHashCode();
            result ^= (this.Member3 == null) ? 0 : this.Member3.GetHashCode();
            result ^= (this.Member5 == null) ? 0 : this.Member5.GetHashCode();
            result ^= this.Member6.GetHashCode();
            result ^= (this.Member7 == null) ? 0 : this.Member7.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_16
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public decimal Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_11 Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_16 other = obj as DCType_16;
            return (other != null) &&
                this.Member0.Equals(other.Member0) &&
                ComparisonHelper.CompareObjects<SerType_11>(this.Member1, other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Member0.GetHashCode();
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_18
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_5 Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public short Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_18 other = obj as DCType_18;
            return (other != null) &&
                ComparisonHelper.CompareObjects<SerType_5>(this.Member0, other.Member0) &&
                this.Member1.Equals(other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= this.Member1.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_19
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        // [CLSCompliant(false)]
        public uint? Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public byte? Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public long? Member2 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_19 other = obj as DCType_19;
            return (other != null) &&
                ComparisonHelper.CompareNullable<uint>(this.Member0, other.Member0) &&
                ComparisonHelper.CompareNullable<byte>(this.Member1, other.Member1) &&
                ComparisonHelper.CompareNullable<long>(this.Member2, other.Member2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            result ^= (this.Member2 == null) ? 0 : this.Member2.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_20
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_9 Member0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_20 other = obj as DCType_20;
            return (other != null) &&
                ComparisonHelper.CompareObjects<DCType_9>(this.Member0, other.Member0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class SerType_22
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public byte Member0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            SerType_22 other = obj as SerType_22;
            return (other != null) &&
                this.Member0.Equals(other.Member0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = this.Member0.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_25
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_22 Member0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_25 other = obj as DCType_25;
            return (other != null) &&
                ComparisonHelper.CompareObjects<SerType_22>(this.Member0, other.Member0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class SerType_26
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public char Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public short? Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public SerType_4 Member2 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public decimal Member3 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public DCType_3 Member4 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            SerType_26 other = obj as SerType_26;
            return (other != null) &&
                this.Member0.Equals(other.Member0) &&
                ComparisonHelper.CompareNullable<short>(this.Member1, other.Member1) &&
                ComparisonHelper.CompareObjects<SerType_4>(this.Member2, other.Member2) &&
                this.Member3.Equals(other.Member3) &&
                ComparisonHelper.CompareObjects<DCType_3>(this.Member4, other.Member4);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Member0.GetHashCode();
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            result ^= (this.Member2 == null) ? 0 : this.Member2.GetHashCode();
            result ^= this.Member3.GetHashCode();
            result ^= (this.Member4 == null) ? 0 : this.Member4.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_31
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_22 Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public byte Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_31 other = obj as DCType_31;
            return (other != null) &&
                ComparisonHelper.CompareObjects<SerType_22>(this.Member0, other.Member0) &&
                this.Member1.Equals(other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= this.Member1.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_32
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public int Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_20 Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_32 other = obj as DCType_32;
            return (other != null) &&
                this.Member0.Equals(other.Member0) &&
                ComparisonHelper.CompareObjects<DCType_20>(this.Member1, other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Member0.GetHashCode();
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class SerType_33
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public long? Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public DCType_20 Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            SerType_33 other = obj as SerType_33;
            return (other != null) &&
                this.Member0.Equals(other.Member0) &&
                ComparisonHelper.CompareObjects<DCType_20>(this.Member1, other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_34
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public short? Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public float Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public EnumType_17 Member2 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_34 other = obj as DCType_34;
            return (other != null) &&
                ComparisonHelper.CompareNullable<short>(this.Member0, other.Member0) &&
                this.Member1.Equals(other.Member1) &&
                this.Member2.Equals(other.Member2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= this.Member1.GetHashCode();
            result ^= this.Member2.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_36
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public bool Member0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_36 other = obj as DCType_36;
            return (other != null) &&
                this.Member0.Equals(other.Member0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = this.Member0.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_38
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        // [CLSCompliant(false)]
        public ulong Member0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_38 other = obj as DCType_38;
            return (other != null) &&
                this.Member0.Equals(other.Member0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = this.Member0.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_40
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_22 Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public short Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_40 other = obj as DCType_40;
            return (other != null) &&
                ComparisonHelper.CompareObjects<SerType_22>(this.Member0, other.Member0) &&
                this.Member1.Equals(other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= this.Member1.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_42
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_11 Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public EnumType_35 Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_5 Member2 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_3 Member3 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_42 other = obj as DCType_42;
            return (other != null) &&
                ComparisonHelper.CompareObjects<SerType_11>(this.Member0, other.Member0) &&
                this.Member1 == other.Member1 &&
                ComparisonHelper.CompareObjects<SerType_5>(this.Member2, other.Member2) &&
                ComparisonHelper.CompareObjects<DCType_3>(this.Member3, other.Member3);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= this.Member1.GetHashCode();
            result ^= (this.Member2 == null) ? 0 : this.Member2.GetHashCode();
            result ^= (this.Member3 == null) ? 0 : this.Member3.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DCType_65
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public ulong? Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_7 Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_36 Member4 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public uint Member5 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_65 other = obj as DCType_65;
            return (other != null) &&
                ComparisonHelper.CompareNullable<ulong>(this.Member0, other.Member0) &&
                ComparisonHelper.CompareObjects<DCType_7>(this.Member1, other.Member1) &&
                ComparisonHelper.CompareObjects<DCType_36>(this.Member4, other.Member4) &&
                this.Member5.Equals(other.Member5);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Member0 == null) ? 0 : this.Member0.GetHashCode();
            result ^= (this.Member1 == null) ? 0 : this.Member1.GetHashCode();
            result ^= (this.Member4 == null) ? 0 : this.Member4.GetHashCode();
            result ^= this.Member5.GetHashCode();
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    [KnownType(typeof(DerivedType))]
    public class BaseType
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public string Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_1 Member1 { get; set; }

        public static BaseType CreateInstance(Random rndGen, CreatorSettings creatorSettings)
        {
            return new DerivedType(rndGen, creatorSettings);
        }

        /// <summary>
        /// Returns a debug representation for this instance.
        /// </summary>
        /// <returns>A debug representation for this instance.</returns>
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "BaseType<Member0={0},Member1={1}>", ComparisonHelper.EscapeString(this.Member0), ComparisonHelper.EscapeString(this.Member1));
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DerivedType : BaseType, IEmptyInterface
    {
        /// <summary>
        /// Default constrcutor that initializes an instance of this type.
        /// </summary>
        public DerivedType()
        {
        }

        /// <summary>
        /// Initializes an instance of this type.
        /// </summary>
        /// <param name="rndGen">The random generator used to populate this type.</param>
        public DerivedType(Random rndGen, CreatorSettings creatorSettings)
        {
            this.Member0 = InstanceCreator.CreateInstanceOf<string>(rndGen, creatorSettings);
            this.Member1 = InstanceCreator.CreateInstanceOf<DCType_1>(rndGen, creatorSettings);
            this.Member2 = InstanceCreator.CreateInstanceOf<SerType_4>(rndGen, creatorSettings);
            this.Member3 = InstanceCreator.CreateInstanceOf<decimal>(rndGen, creatorSettings);
        }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_4 Member2 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public decimal Member3 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DerivedType other = obj as DerivedType;
            return (other != null) &&
                ComparisonHelper.CompareObjects<string>(this.Member0, other.Member0) &&
                ComparisonHelper.CompareObjects<DCType_1>(this.Member1, other.Member1) &&
                ComparisonHelper.CompareObjects<SerType_4>(this.Member2, other.Member2) &&
                this.Member3.Equals(other.Member3);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Member0 == null ? 0 : this.Member0.GetHashCode();
            result ^= this.Member1 == null ? 0 : this.Member1.GetHashCode();
            result ^= this.Member2 == null ? 0 : this.Member2.GetHashCode();
            result ^= this.Member3.GetHashCode();
            return result;
        }

        /// <summary>
        /// Returns a debug representation for this instance.
        /// </summary>
        /// <returns>A debug representation for this instance.</returns>
        public override string ToString()
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                "DerivedType<Base={0},Member2={1},Member3={2}>",
                base.ToString(),
                ComparisonHelper.EscapeString(this.Member2),
                ComparisonHelper.EscapeString(this.Member3));
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public string Street { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public string State { get; set; }

        /// <summary>
        /// Returns a readable representation of a <see cref="Address"/> instance.
        /// </summary>
        /// <returns>A readable representation of this instance.</returns>
        public override string ToString()
        {
            return string.Format("Address{{{0}, {1}, {2}}}", !String.IsNullOrEmpty(this.Street) ? this.Street : string.Empty,
                !String.IsNullOrEmpty(this.City) ? this.City : string.Empty,
                !String.IsNullOrEmpty(this.State) ? this.State : string.Empty);
        }

        public override bool Equals(object obj)
        {
            Address other = obj as Address;
            return other != null && this.ToString() == other.ToString();
        }

        public override int GetHashCode()
        {
            return this.ToString().GetHashCode();
        }
    }

    //POCO types without explicitly defining the constructors.  These types are used in the real http://asgtestsrv.
    public class TestRun
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string RunCategory { get; set; }
        public bool RunVisible { get; set; }
        public string Owner { get; set; }
        public string Group { get; set; }
        public string RunInfo { get; set; }
        public bool ActiveForAnalysis { get; set; }
        public string SignedOff { get; set; }
        public int Priority { get; set; }
        public string TcmName { get; set; }
        public string TcmDatabase { get; set; }
        public string ConfigName { get; set; }
        public string Attributes { get; set; }
        public string ImportantNotes { get; set; }
        public SuitePlan SuitePlan { get; set; }

        // These properties are necessary for Binding on TestRuns.aspx
        public string ProductName { get; set; }
        public string ProductVersion { get; set; }
        public string OSName { get; set; }
        public string OSArchitecture { get; set; }
        public string OSLanguage { get; set; }

        public bool Analyzed
        {
            get
            {
                if (this.SignedOff != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public Branch Product
        {
            get
            {
                return new Branch
                {
                    Name = this.ProductName,
                    Version = this.ProductVersion
                };
            }
            set
            {
                if (value != null)
                {
                    this.ProductName = value.Name;
                    this.ProductVersion = value.Version;
                }
            }
        }

        public OS OS
        {
            get
            {
                return new OS
                {
                    Name = this.OSName,
                    Architecture = this.OSArchitecture,
                    Language = this.OSLanguage
                };
            }
            set
            {
                if (value != null)
                {
                    this.OSName = value.Name;
                    this.OSArchitecture = value.Architecture;
                    this.OSLanguage = value.Language;
                }
            }
        }

        // For binding on Admin/TestRuns.aspx
        public int? SuitePlanId
        {
            get
            {
                return (this.SuitePlan != null) ? this.SuitePlan.ID : (int?)null;
            }
        }

        public string LastSyncData { get; set; }

        public override bool Equals(object obj)
        {
            TestRun other = obj as TestRun;
            return (other != null) &&
                this.ActiveForAnalysis.Equals(other.ActiveForAnalysis) &&
                this.ID.Equals(other.ID) &&
                this.Priority.Equals(other.Priority) &&
                this.RunVisible.Equals(other.RunVisible) &&
                ComparisonHelper.CompareObjects<string>(this.Attributes, other.Attributes) &&
                ComparisonHelper.CompareObjects<string>(this.ConfigName, other.ConfigName) &&
                ComparisonHelper.CompareObjects<string>(this.Description, other.Description) &&
                ComparisonHelper.CompareObjects<string>(this.Group, other.Group) &&
                ComparisonHelper.CompareObjects<string>(this.ImportantNotes, other.ImportantNotes) &&
                ComparisonHelper.CompareObjects<string>(this.LastSyncData, other.LastSyncData) &&
                ComparisonHelper.CompareObjects<OS>(this.OS, other.OS) &&
                ComparisonHelper.CompareObjects<string>(this.OSArchitecture, other.OSArchitecture) &&
                ComparisonHelper.CompareObjects<string>(this.OSLanguage, other.OSLanguage) &&
                ComparisonHelper.CompareObjects<string>(this.OSName, other.OSName) &&
                ComparisonHelper.CompareObjects<string>(this.Owner, other.Owner) &&
                ComparisonHelper.CompareObjects<Branch>(this.Product, other.Product) &&
                ComparisonHelper.CompareObjects<string>(this.ProductName, other.ProductName) &&
                ComparisonHelper.CompareObjects<string>(this.ProductVersion, other.ProductVersion) &&
                ComparisonHelper.CompareObjects<string>(this.RunCategory, other.RunCategory) &&
                ComparisonHelper.CompareObjects<string>(this.RunInfo, other.RunInfo) &&
                ComparisonHelper.CompareObjects<string>(this.SignedOff, other.SignedOff) &&
                ComparisonHelper.CompareObjects<SuitePlan>(this.SuitePlan, other.SuitePlan) &&
                ComparisonHelper.CompareObjects<string>(this.TcmDatabase, other.TcmDatabase) &&
                ComparisonHelper.CompareObjects<string>(this.TcmName, other.TcmName) &&
                ComparisonHelper.CompareObjects<string>(this.Title, other.Title);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.ActiveForAnalysis.GetHashCode();
            result ^= (this.Attributes == null) ? 0 : this.Attributes.GetHashCode();
            result ^= (this.ConfigName == null) ? 0 : this.ConfigName.GetHashCode();
            result ^= (this.Description == null) ? 0 : this.Description.GetHashCode();
            result ^= (this.Group == null) ? 0 : this.Group.GetHashCode();
            result ^= this.ID.GetHashCode();
            result ^= (this.ImportantNotes == null) ? 0 : this.ImportantNotes.GetHashCode();
            result ^= (this.LastSyncData == null) ? 0 : this.LastSyncData.GetHashCode();
            result ^= (this.OS == null) ? 0 : this.OS.GetHashCode();
            result ^= (this.OSArchitecture == null) ? 0 : this.OSArchitecture.GetHashCode();
            result ^= (this.OSLanguage == null) ? 0 : this.OSLanguage.GetHashCode();
            result ^= (this.OSName == null) ? 0 : this.OSName.GetHashCode();
            result ^= (this.Owner == null) ? 0 : this.Owner.GetHashCode();
            result ^= this.Priority.GetHashCode();
            result ^= (this.Product == null) ? 0 : this.Product.GetHashCode();
            result ^= (this.ProductName == null) ? 0 : this.ProductName.GetHashCode();
            result ^= (this.ProductVersion == null) ? 0 : this.ProductVersion.GetHashCode();
            result ^= (this.RunCategory == null) ? 0 : this.RunCategory.GetHashCode();
            result ^= (this.RunInfo == null) ? 0 : this.RunInfo.GetHashCode();
            result ^= this.RunVisible.GetHashCode();
            result ^= (this.SignedOff == null) ? 0 : this.SignedOff.GetHashCode();
            result ^= (this.SuitePlan == null) ? 0 : this.SuitePlan.GetHashCode();
            result ^= (this.TcmDatabase == null) ? 0 : this.TcmDatabase.GetHashCode();
            result ^= (this.TcmName == null) ? 0 : this.TcmName.GetHashCode();
            result ^= (this.Title == null) ? 0 : this.Title.GetHashCode();
            return result;
        }
    }

    public class Branch
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }

        public override bool Equals(object obj)
        {
            Branch other = obj as Branch;
            return (other != null) &&
                this.ID.Equals(other.ID) &&
                ComparisonHelper.CompareObjects<string>(this.Name, other.Name) &&
                ComparisonHelper.CompareObjects<string>(this.Version, other.Version) &&
                ComparisonHelper.CompareObjects<string>(this.Description, other.Description);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.ID.GetHashCode();
            result ^= (this.Name == null) ? 0 : this.Name.GetHashCode();
            result ^= (this.Version == null) ? 0 : this.Version.GetHashCode();
            result ^= (this.Description == null) ? 0 : this.Description.GetHashCode();
            return result;
        }
    }

    public class OS
    {
        public string Name { get; set; }
        public string Architecture { get; set; }
        public string Language { get; set; }

        public override bool Equals(object obj)
        {
            OS other = obj as OS;
            return (other != null) &&
                ComparisonHelper.CompareObjects<string>(this.Name, other.Name) &&
                ComparisonHelper.CompareObjects<string>(this.Architecture, other.Architecture) &&
                ComparisonHelper.CompareObjects<string>(this.Language, other.Language);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result ^= (this.Name == null) ? 0 : this.Name.GetHashCode();
            result ^= (this.Architecture == null) ? 0 : this.Architecture.GetHashCode();
            result ^= (this.Language == null) ? 0 : this.Language.GetHashCode();
            return result;
        }
    }

    public class SuitePlan
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string SuiteInfo { get; set; }
        public string ETCMDatabase { get; set; }

        public override bool Equals(object obj)
        {
            SuitePlan other = obj as SuitePlan;
            return (other != null) &&
                this.ID.Equals(other.ID) &&
                ComparisonHelper.CompareObjects<string>(this.Name, other.Name) &&
                ComparisonHelper.CompareObjects<string>(this.SuiteInfo, other.SuiteInfo) &&
                ComparisonHelper.CompareObjects<string>(this.ETCMDatabase, other.ETCMDatabase);
        }

        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.ID.GetHashCode();
            result ^= (this.Name == null) ? 0 : this.Name.GetHashCode();
            result ^= (this.SuiteInfo == null) ? 0 : this.SuiteInfo.GetHashCode();
            result ^= (this.ETCMDatabase == null) ? 0 : this.ETCMDatabase.GetHashCode();
            return result;
        }
    }
}
