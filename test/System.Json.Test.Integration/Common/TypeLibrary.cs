// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace System.Json
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
        public short Int16Member;
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
        public Guid GuidMember;
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
            return String.Format(CultureInfo.InvariantCulture, "DCType_1<Member0={0:X2}>", (int)this.Member0);
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [CLSCompliant(false)]
    [DataContract]
    public class DCType_3
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
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
    [Serializable]
    public class SerType_4
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public char Member0;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public short? Member1;

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            SerType_4 other = obj as SerType_4;
            return (other != null) && this.Member0.Equals(other.Member0) && Util.CompareNullable<short>(this.Member1, other.Member1);
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
            return String.Format(CultureInfo.InvariantCulture, "SerType_4<Member0=(char){0},Member1={1}>", (int)this.Member0, Util.EscapeString(this.Member1));
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    public class SerType_5
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public char Member0;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public byte? Member1;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public char Member2;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public bool Member3;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public sbyte Member4;

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
                Util.CompareNullable<byte>(this.Member1, other.Member1) &&
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
    [CLSCompliant(false)]
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
        public sbyte Member2 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public byte[] Member3 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_7 other = obj as DCType_7;
            return (other != null) &&
                Util.CompareNullable<long>(this.Member1, other.Member1) &&
                this.Member2.Equals(other.Member2) &&
                Util.CompareArrays(this.Member3, other.Member3);
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
            result ^= Util.ComputeArrayHashCode(this.Member3);
            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [CLSCompliant(false)]
    [DataContract]
    public class DCType_9
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
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
                Util.CompareNullable<sbyte>(this.Member0, other.Member0) &&
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
    [Serializable]
    public class SerType_11
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public float Member0;

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
    [CLSCompliant(false)]
    [DataContract]
    public class DCType_15
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public byte[] Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public ushort Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public Guid Member2 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
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
                this.Member2.Equals(other.Member2) &&
                Util.CompareObjects<DCType_7>(this.Member5, other.Member5) &&
                this.Member6.Equals(other.Member6) &&
                Util.CompareObjects<DCType_9>(this.Member7, other.Member7);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.Member2.GetHashCode();
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
                Util.CompareObjects<SerType_11>(this.Member1, other.Member1);
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
    [CLSCompliant(false)]
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
                Util.CompareObjects<SerType_5>(this.Member0, other.Member0) &&
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
    [CLSCompliant(false)]
    [DataContract]
    public class DCType_19
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
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
                Util.CompareNullable<uint>(this.Member0, other.Member0) &&
                Util.CompareNullable<byte>(this.Member1, other.Member1) &&
                Util.CompareNullable<long>(this.Member2, other.Member2);
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
    [CLSCompliant(false)]
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
                Util.CompareObjects<DCType_9>(this.Member0, other.Member0);
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
    [Serializable]
    public class SerType_22
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public byte Member0;

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
                Util.CompareObjects<SerType_22>(this.Member0, other.Member0);
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
    [CLSCompliant(false)]
    [Serializable]
    public class SerType_26
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public char Member0;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public short? Member1;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public SerType_4 Member2;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public decimal Member3;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        [SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields",
            Justification = "The type is serializable (it contains a [DataContract] attribute).")]
        public DCType_3 Member4;

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
                Util.CompareNullable<short>(this.Member1, other.Member1) &&
                Util.CompareObjects<SerType_4>(this.Member2, other.Member2) &&
                this.Member3.Equals(other.Member3) &&
                Util.CompareObjects<DCType_3>(this.Member4, other.Member4);
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
                Util.CompareObjects<SerType_22>(this.Member0, other.Member0) &&
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
    [CLSCompliant(false)]
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
                Util.CompareObjects<DCType_20>(this.Member1, other.Member1);
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
    [CLSCompliant(false)]
    [Serializable]
    public class SerType_33
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        public long? Member0;

        /// <summary>
        /// Test member.
        /// </summary>
        [SuppressMessage("Microsoft.StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate",
            Justification = "Testing serialization of [Serializable] types, which needs public fields.")]
        [SuppressMessage("Microsoft.Usage", "CA2235:MarkAllNonSerializableFields",
            Justification = "The type is serializable (it contains a [DataContract] attribute).")]
        public DCType_20 Member1;

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
                Util.CompareObjects<DCType_20>(this.Member1, other.Member1);
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
                Util.CompareNullable<short>(this.Member0, other.Member0) &&
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
        public bool? Member0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            DCType_36 other = obj as DCType_36;
            return (other != null) &&
                Util.CompareNullable<bool>(this.Member0, other.Member0);
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
    [CLSCompliant(false)]
    [DataContract]
    public class DCType_38
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
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
                Util.CompareObjects<SerType_22>(this.Member0, other.Member0) &&
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
    [CLSCompliant(false)]
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
                Util.CompareObjects<SerType_11>(this.Member0, other.Member0) &&
                this.Member1.Equals(other.Member1) &&
                Util.CompareObjects<SerType_5>(this.Member2, other.Member2) &&
                Util.CompareObjects<DCType_3>(this.Member3, other.Member3);
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
    [CLSCompliant(false)]
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
                Util.CompareNullable<ulong>(this.Member0, other.Member0) &&
                Util.CompareObjects<DCType_7>(this.Member1, other.Member1) &&
                Util.CompareObjects<DCType_36>(this.Member4, other.Member4) &&
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
    [CLSCompliant(false)]
    public class ListType_1
    {
        /// <summary>
        /// Test member.
        /// </summary>
        public List<DCType_15> Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public List<DCType_34> Member1 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public List<SerType_33> Member2 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            ListType_1 other = obj as ListType_1;
            return (other != null) &&
                Util.CompareLists<DCType_15>(this.Member0, other.Member0) &&
                Util.CompareLists<DCType_34>(this.Member1, other.Member1) &&
                Util.CompareLists<SerType_33>(this.Member2, other.Member2);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            if (this.Member0 != null)
            {
                result ^= Util.ComputeArrayHashCode(this.Member0.ToArray());
            }

            if (this.Member1 != null)
            {
                result ^= Util.ComputeArrayHashCode(this.Member0.ToArray());
            }

            if (this.Member2 != null)
            {
                result ^= Util.ComputeArrayHashCode(this.Member0.ToArray());
            }

            return result;
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [CLSCompliant(false)]
    [DataContract]
    public class ListType_2
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public SerType_4[] Member0 { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public DCType_32[] Member1 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            ListType_2 other = obj as ListType_2;
            return (other != null) &&
                Util.CompareArrays(this.Member0, other.Member0) &&
                Util.CompareArrays(this.Member1, other.Member1);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= Util.ComputeArrayHashCode(this.Member0);
            result ^= Util.ComputeArrayHashCode(this.Member0);
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

        /// <summary>
        /// Creates an instance of this type.
        /// </summary>
        /// <param name="rndGen">The random generator used to populate this type.</param>
        /// <returns>An instance of the <see cref="DerivedType"/>.</returns>
        public static BaseType CreateInstance(Random rndGen)
        {
            return new DerivedType(rndGen);
        }

        /// <summary>
        /// Returns a debug representation for this instance.
        /// </summary>
        /// <returns>A debug representation for this instance.</returns>
        public override string ToString()
        {
            return String.Format(CultureInfo.InvariantCulture, "BaseType<Member0={0},Member1={1}>", Util.EscapeString(this.Member0), Util.EscapeString(this.Member1));
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class DerivedType : BaseType, IEmptyInterface
    {
        /// <summary>
        /// Initializes an instance of this type.
        /// </summary>
        /// <param name="rndGen">The random generator used to populate this type.</param>
        public DerivedType(Random rndGen)
        {
            this.Member0 = InstanceCreator.CreateInstanceOf<string>(rndGen);
            this.Member1 = InstanceCreator.CreateInstanceOf<DCType_1>(rndGen);
            this.Member2 = InstanceCreator.CreateInstanceOf<SerType_4>(rndGen);
            this.Member3 = InstanceCreator.CreateInstanceOf<decimal>(rndGen);
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
                Util.CompareObjects<string>(this.Member0, other.Member0) &&
                Util.CompareObjects<DCType_1>(this.Member1, other.Member1) &&
                Util.CompareObjects<SerType_4>(this.Member2, other.Member2) &&
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
            return String.Format(
                CultureInfo.InvariantCulture,
                "DerivedType<Base={0},Member2={1},Member3={2}>",
                base.ToString(),
                Util.EscapeString(this.Member2),
                Util.EscapeString(this.Member3));
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    public class PolymorphicMember
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public BaseType Member_0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            PolymorphicMember other = obj as PolymorphicMember;
            return (other != null) &&
                Util.CompareObjects<BaseType>(this.Member_0, other.Member_0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.Member_0 == null ? 0 : this.Member_0.GetHashCode();
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract, KnownType(typeof(DerivedType))]
    public class PolymorphicAsInterfaceMember
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public IEmptyInterface Member_0 { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            PolymorphicAsInterfaceMember other = obj as PolymorphicAsInterfaceMember;
            return (other != null) &&
                Util.CompareObjects<IEmptyInterface>(this.Member_0, other.Member_0);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            return this.Member_0 == null ? 0 : this.Member_0.GetHashCode();
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    [DataContract]
    [KnownType(typeof(DerivedType))]
    public class CollectionsWithPolymorphicMember
    {
        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public List<BaseType> ListOfBase { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public List<IEmptyInterface> ListOfInterface { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public Dictionary<string, BaseType> DictionaryOfBase { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        [DataMember]
        public Dictionary<string, IEmptyInterface> DictionaryOfInterface { get; set; }

        /// <summary>
        /// Compares this instance with the given object.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns><code>true</code> if the given instance is equal to this one; <code>false</code> otherwise.</returns>
        public override bool Equals(object obj)
        {
            CollectionsWithPolymorphicMember other = obj as CollectionsWithPolymorphicMember;
            return (other != null) &&
                Util.CompareLists<BaseType>(this.ListOfBase, other.ListOfBase) &&
                Util.CompareLists<IEmptyInterface>(this.ListOfInterface, other.ListOfInterface) &&
                Util.CompareDictionaries<string, BaseType>(this.DictionaryOfBase, other.DictionaryOfBase) &&
                Util.CompareDictionaries<string, IEmptyInterface>(this.DictionaryOfInterface, other.DictionaryOfInterface);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance.</returns>
        public override int GetHashCode()
        {
            int result = 0;
            result ^= this.ListOfBase == null ? 0 : Util.ComputeArrayHashCode(this.ListOfBase.ToArray());
            result ^= this.ListOfInterface == null ? 0 : Util.ComputeArrayHashCode(this.ListOfInterface.ToArray());
            result ^= this.DictionaryOfBase == null ? 0 : Util.ComputeArrayHashCode(new List<string>(this.DictionaryOfBase.Keys).ToArray());
            result ^= this.DictionaryOfBase == null ? 0 : Util.ComputeArrayHashCode(new List<BaseType>(this.DictionaryOfBase.Values).ToArray());
            result ^= this.DictionaryOfInterface == null ? 0 : Util.ComputeArrayHashCode(new List<string>(this.DictionaryOfInterface.Keys).ToArray());
            result ^= this.DictionaryOfInterface == null ? 0 : Util.ComputeArrayHashCode(new List<IEmptyInterface>(this.DictionaryOfInterface.Values).ToArray());
            return result;
        }

        /// <summary>
        /// Returns a debug representation for this instance.
        /// </summary>
        /// <returns>A debug representation for this instance.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("CollectionsWithPolymorphicMember<");
            PrintList(sb, "ListOfBase", this.ListOfBase);
            sb.Append(", ");
            PrintList(sb, "ListOfInterface", this.ListOfBase);
            sb.Append(", ");
            PrintDictionary(sb, "DictionaryOfBase", this.DictionaryOfBase);
            sb.Append(", ");
            PrintDictionary(sb, "DictionaryOfInterface", this.DictionaryOfInterface);
            sb.Append('>');
            return sb.ToString();
        }

        private static void PrintList<T>(StringBuilder sb, string name, List<T> list)
        {
            sb.Append(name);
            sb.Append('=');
            if (list == null)
            {
                sb.Append("<<null>>");
            }
            else
            {
                sb.Append('[');
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(',');
                    }

                    sb.Append(Util.EscapeString(list[i]));
                }

                sb.Append(']');
            }
        }

        private static void PrintDictionary<T>(StringBuilder sb, string name, Dictionary<string, T> dict)
        {
            sb.Append(name);
            sb.Append('=');
            if (dict == null)
            {
                sb.Append("<<null>>");
            }
            else
            {
                sb.Append('{');
                bool first = true;
                foreach (string key in dict.Keys)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        sb.Append(',');
                    }

                    sb.AppendFormat("\"{0}\":", Util.EscapeString(key));
                    sb.Append(Util.EscapeString(dict[key]));
                }

                sb.Append('}');
            }
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class Person
    {
        internal const string Letters = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        public Person()
        {
            this.Friends = new List<Person>();
        }

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        /// <param name="rndGen">The random generator used to populate this type.</param>
        public Person(Random rndGen)
        {
            this.Name = PrimitiveCreator.CreateInstanceOfString(rndGen, rndGen.Next(5, 15), Letters);
            this.Age = PrimitiveCreator.CreateInstanceOfInt32(rndGen);
            this.Address = new Address(rndGen);
            this.Friends = new List<Person>();
        }

        /// <summary>
        /// Test member.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public Address Address { get; set; }

        /// <summary>
        /// Test member.
        /// </summary>
        public List<Person> Friends { get; set; }

        /// <summary>
        /// Adds new instances of <see cref="Person"/> as <see cref="Person.Friends"/> of this instance.
        /// </summary>
        /// <param name="count">The number of instances to add.</param>
        /// <param name="rndGen">The random generator used to populate the instances.</param>
        public void AddFriends(int count, Random rndGen)
        {
            for (int i = 0; i < count; i++)
            {
                this.Friends.Add(new Person(rndGen));
            }
        }

        /// <summary>
        /// Returns a string representation of the "friends" of this instance, for logging purposes.
        /// </summary>
        /// <returns>A string representation of the "friends" of this instance.</returns>
        public string FriendsToString()
        {
            string s = "";

            foreach (Person p in this.Friends)
            {
                s += p + ",";
            }

            return s;
        }

        /// <summary>
        /// Returns a readable representation of a <see cref="Person"/> instance.
        /// </summary>
        /// <returns>A readable representation of this instance.</returns>
        public override string ToString()
        {
            return String.Format("Person{{{0}, {1}, [{2}], Friends=[{3}]}}", this.Name, this.Age, this.Address, this.FriendsToString());
        }
    }

    /// <summary>
    /// Test type.
    /// </summary>
    public class Address
    {
        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        public Address()
        {
        }

        /// <summary>
        /// Initializes an instance of this class.
        /// </summary>
        /// <param name="rndGen">The random generator used to populate this type.</param>
        public Address(Random rndGen)
        {
            this.Street = PrimitiveCreator.CreateInstanceOfString(rndGen, rndGen.Next(5, 15), Person.Letters);
            this.City = PrimitiveCreator.CreateInstanceOfString(rndGen, rndGen.Next(5, 15), Person.Letters);
            this.State = PrimitiveCreator.CreateInstanceOfString(rndGen, rndGen.Next(5, 15), Person.Letters);
        }

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
            return String.Format("Address{{{0}, {1}, {2}}}", this.Street, this.City, this.State);
        }
    }
}

